using EP94.LgSmartThinq.Models;
using EP94.LgSmartThinq.Utils;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public delegate void ErrorHandler(Exception exception);
    public delegate void ConnectionStatusChangeHandler(bool connected);
    public class ThinqMqttClient : IDisposable
    {
        //internal static ThinqMqttClient GetOrCreate(string brokerAddress, ThinqClient thinqClient)
        //{
        //    if (_mqttClient == null)
        //        _mqttClient = new ThinqMqttClient(brokerAddress, thinqClient);
        //    return _mqttClient;
        //}
        //private static ThinqMqttClient _mqttClient;

        public event ErrorHandler OnError;
        public event ConnectionStatusChangeHandler OnConnectionStatusChange;
        public bool Connected => _client?.IsConnected ?? false;
        private Uri _brokerUri;
        private ThinqClient _thinqClient;
        private IMqttClient _client = null;
        private Dictionary<Device, List<Action<Snapshot>>> _subscriptions = new Dictionary<Device, List<Action<Snapshot>>>();
        private int _timeout = 1;
        private bool _disposed = false;

        internal ThinqMqttClient(string brokerAddress, ThinqClient thinqClient)
        {
            _brokerUri = new Uri(brokerAddress);
            _thinqClient = thinqClient;
        }

        public void SubscribeToChanges(Device device, Action<Snapshot> action)
        {
            if (!_subscriptions.ContainsKey(device))
                _subscriptions.Add(device, new List<Action<Snapshot>>());

            _subscriptions[device].Add(action);
        }

        public async Task<bool> Connect(bool autoReconnect = true)
        {
            SmartThinqLogger.Log("Start creating MQTT session", LogLevel.Information);
            if (_client != null && _client.IsConnected)
            {
                SmartThinqLogger.Log("Client already connected", LogLevel.Debug);
                return true;
            }
            try
            {
                IotCertificateRegisterResponse registerResponse = await _thinqClient.RegisterIotCertificate(X509CertificateHelpers.CreateCsr(out AsymmetricCipherKeyPair keyPair));
                SmartThinqLogger.Log("IOT certificate registered", LogLevel.Debug);
                using X509Certificate2 certificate = registerResponse.CertificatePem;

                RSA rsa;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    rsa = DotNetUtilities.ToRSA(keyPair.Private as RsaPrivateCrtKeyParameters);
                }
                else
                {
                    RSAParameters parameters = DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);
                    rsa = RSA.Create();
                    rsa.ImportParameters(parameters);
                }
                
                X509Certificate2 certWithPrivateKey = certificate.CopyWithPrivateKey(rsa);

                if (_client != null)
                    _client.Dispose();

                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();
                var clientOptions = new MqttClientOptions
                {
                    ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = _brokerUri.Host,
                        Port = _brokerUri.Port,
                        TlsOptions = new MqttClientTlsOptions()
                        {
                            UseTls = true,
                            AllowUntrustedCertificates = true,

                            Certificates = new List<X509Certificate>() { certWithPrivateKey },
                            CertificateValidationHandler = (c) =>
                            {
                                return true;
                            },
                            SslProtocol = SslProtocols.None
                        }
                    },
                    ClientId = Constants.CLIENT_ID
                };
                _client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
                {
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    SmartThinqLogger.Log("New MQTT message received: {0}", LogLevel.Verbose, payload);
                    JObject jObject = JsonConvert.DeserializeObject(payload) as JObject;
                    if (jObject.TryGetValue("controlResult", out JToken value))
                    {
                        string returnCode = value["returnCode"].ToObject<string>();
                        SmartThinqLogger.Log("Error sent by device, code: {0}", LogLevel.Verbose, returnCode);
                        return;
                    }
                    string deviceId = jObject["deviceId"].ToObject<string>();
                    Snapshot snapshot = jObject["data"]["state"]["reported"].ToObject<Snapshot>();
                    Device device = _subscriptions.Keys.FirstOrDefault(d => d.DeviceId.Equals(deviceId));
                    if (device != null)
                    {
                        SmartThinqLogger.Log("{0} subscribers found for device {1}", LogLevel.Verbose, _subscriptions[device].Count, device);
                        _subscriptions[device].ForEach(a => a?.Invoke(snapshot));
                    }
                });
                _client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(async e =>
                {
                    _timeout = 1;
                    SmartThinqLogger.Log("Connected to MQTT address {0}:{1}", LogLevel.Information, _brokerUri.Host, _brokerUri.Port);
                    OnConnectionStatusChange?.Invoke(true);
                    foreach (var subscription in registerResponse.Subscriptions)
                        await _client.SubscribeAsync(new MqttTopicFilter() { Topic = subscription });
                });
                _client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
                {
                    SmartThinqLogger.Log("Disconnected to MQTT address {0}:{1}", LogLevel.Information, _brokerUri.Host, _brokerUri.Port);
                    OnConnectionStatusChange?.Invoke(false);
                    if (autoReconnect)
                    {
                        SmartThinqLogger.Log("Reconnecting to MQTT address {0}:{1}...", LogLevel.Information, _brokerUri.Host, _brokerUri.Port);
                        while (!_client.IsConnected)
                        {
                            _timeout = Math.Min(_timeout * 2, 30);
                            Thread.Sleep(TimeSpan.FromSeconds(_timeout));
                            if (!_disposed)
                            {
                                await _client.ConnectAsync(clientOptions);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                });
                var result = await _client.ConnectAsync(clientOptions);
                bool success = result.ResultCode == MqttClientConnectResultCode.Success;
                return success;
            }
            catch (Exception e)
            {
                SmartThinqLogger.Log("Exception: {0}", LogLevel.Error, e);
                OnError?.Invoke(e);
                return false;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
            _disposed = true;
        }
    }
}
