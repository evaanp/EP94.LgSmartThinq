using EP94.LgSmartThinq.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public abstract class DeviceClient : ThinqApiClient
    {
        private string _deviceId;
        protected DeviceClient(Passport passport, string baseUrl, string deviceId, OAuthClient oAuthClient) : base(passport, baseUrl, oAuthClient)
        {
            _deviceId = deviceId;
        }

        protected async Task<bool> SendCommand(string command, string dataKey, object dataValue)
        {
            using HttpRequestMessage httpRequestMessage = GetHttpRequestMessage(HttpMethod.Post, $"/service/devices/{_deviceId}/control-sync");
            Dictionary<string, string> commandDict = new Dictionary<string, string>
            {
                { "command", command },
                { "dataKey", dataKey },
                { "dataValue", dataValue.ToString() },
                { "ctrlKey", "basicCtrl" }
            };
            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(commandDict));
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await ExecuteRequest(httpRequestMessage);
        }

        protected async Task<Snapshot> GetDeviceSnapshot()
        {
            using HttpRequestMessage httpRequestMessage = GetHttpRequestMessage(HttpMethod.Get, $"/service/devices/{_deviceId}");
            Device device = await ExecuteRequest<Device>(httpRequestMessage);
            return device.Snapshot;
        }
    }
}
