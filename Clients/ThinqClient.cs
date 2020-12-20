using EP94.LgSmartThinq.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    internal class ThinqClient : ThinqApiClient
    {
        public ThinqClient(Passport passport, string baseUrl, OAuthClient oAuthClient) : base(passport, baseUrl, oAuthClient) { }

        public async Task<List<Device>> GetDevices()
        {
            HttpRequestMessage requestMessage = GetHttpRequestMessage(HttpMethod.Get, "/service/application/dashboard");
            return await ExecuteRequest<List<Device>>(requestMessage, "item");
        }

        public async Task<IotCertificateRegisterResponse> RegisterIotCertificate(string csr)
        {
            await CheckDeviceRegistration();
            using HttpRequestMessage requestMessage = GetHttpRequestMessage(HttpMethod.Post, "/service/users/client/certificate");
            Dictionary<string, string> msg = new Dictionary<string, string>() { { "csr", csr } };
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(msg));
            requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
            var response = await ExecuteRequest<Dictionary<string, object>>(requestMessage);
            return new IotCertificateRegisterResponse() { CertificatePem = new X509Certificate2(Encoding.UTF8.GetBytes(response["certificatePem"].ToString())), Subscriptions = ((JArray)response["subscriptions"]).ToObject<List<string>>() };
        }

        private async Task CheckDeviceRegistration()
        {
            bool isDeviceRegistered = await IsDeviceRegistered();
            if (!isDeviceRegistered)
            {
                await RegisterDevice();
            }
        }

        private async Task<bool> IsDeviceRegistered()
        {
            using HttpRequestMessage requestMessage = GetHttpRequestMessage(HttpMethod.Get, "/service/users/client");
            return await ExecuteRequestResultCode(requestMessage) != ErrorCodes.NOT_EXIST_DATA;
        }

        private async Task RegisterDevice()
        {
            using HttpRequestMessage requestMessage = GetHttpRequestMessage(HttpMethod.Post, "/service/users/client");
            await ExecuteRequest(requestMessage);
        }
    }
}
