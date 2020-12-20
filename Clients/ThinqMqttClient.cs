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
    public class ThinqMqttClient
    {
        private Uri _brokerUri;
        private ThinqClient _thinqClient;
        private IMqttClient _client;
        private List<string> _subscriptions;

        internal ThinqMqttClient(string brokerAddress, ThinqClient thinqClient)
        {
            _brokerUri = new Uri(brokerAddress);
            _thinqClient = thinqClient;
            MqttNetConsoleLogger.ForwardToConsole();
        }

        public async Task Connect()
        {
            try
            {
                X509Certificate2 iotCertificate = new X509Certificate2(Encoding.UTF8.GetBytes(await GetIotCertificate()));
                IotCertificateRegisterResponse registerResponse = await _thinqClient.RegisterIotCertificate(X509CertificateHelpers.CreateCsr(out AsymmetricCipherKeyPair keyPair));
                X509Certificate2 certificate = registerResponse.CertificatePem;
                RSA rsa = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)keyPair.Private);
                X509Certificate2 certWithPrivateKey = certificate.CopyWithPrivateKey(rsa);
                _subscriptions = registerResponse.Subscriptions;

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
                            
                            Certificates = new List<X509Certificate>() { certWithPrivateKey, iotCertificate },
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

                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {payload}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();
                });
                _client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(async e =>
                {
                    foreach (var subscription in registerResponse.Subscriptions)
                        await _client.SubscribeAsync(new MqttTopicFilter() { Topic = subscription });
                });
                _client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(async e =>
                {
                    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                    var result = await _client.ConnectAsync(clientOptions);
                });
                
                try
                {
                    var result = await _client.ConnectAsync(clientOptions);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task<string> GetIotCertificate()
        {
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(Constants.AWS_IOTT_CA_CERT_URL);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
