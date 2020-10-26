using EP94.LgSmartThinq.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    internal class ThinqClient : ThinqApiClient
    {
        public ThinqClient(Passport passport, string baseUrl) : base(passport, baseUrl) { }

        public async Task<List<Device>> GetDevices()
        {
            HttpRequestMessage requestMessage = GetHttpRequestMessage(HttpMethod.Get, "/service/application/dashboard");
            return await ExecuteRequest<List<Device>>(requestMessage, "item");
        }
    }
}
