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
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public delegate void NewDataHandler(Snapshot data);
    public delegate void ErrorHandler(Exception exception);
    public class ThinqMqttClient
    {
        public event NewDataHandler OnNewData;
        public event ErrorHandler OnError;
        public bool Connected = false;
        private Uri _brokerUri;
        private ThinqClient _thinqClient;
        private List<string> _subscriptions;

        internal ThinqMqttClient(string brokerAddress, ThinqClient thinqClient)
        {
            _brokerUri = new Uri(brokerAddress);
            _thinqClient = thinqClient;
        }

        public async Task Connect(bool autoReconnect = true)
        {
            try
            {
                IotCertificateRegisterResponse registerResponse = await _thinqClient.RegisterIotCertificate(X509CertificateHelpers.CreateCsr(out AsymmetricCipherKeyPair keyPair));
                X509Certificate2 certificate = registerResponse.CertificatePem;
                RSA rsa = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)keyPair.Private);
                X509Certificate2 certWithPrivateKey = certificate.CopyWithPrivateKey(rsa);
                _subscriptions = registerResponse.Subscriptions;

                var factory = new MqttFactory();
                using IMqttClient client = factory.CreateMqttClient();
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
                client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
                {
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    JObject jObject = JsonConvert.DeserializeObject(payload) as JObject;
                    Snapshot snapshot = jObject["data"]["state"]["reported"].ToObject<Snapshot>();
                    OnNewData?.Invoke(snapshot);
                });
                client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(async e =>
                {
                    Connected = true;
                    foreach (var subscription in registerResponse.Subscriptions)
                        await client.SubscribeAsync(new MqttTopicFilter() { Topic = subscription });
                });
                client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
                {
                    Connected = false;
                    if (autoReconnect)
                        await client.ConnectAsync(clientOptions);
                });
                await client.ConnectAsync(clientOptions);
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
        }
    }
}
