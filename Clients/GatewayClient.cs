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
    internal class GatewayClient : ThinqApiClient
    {
        public GatewayClient(Passport passport, OAuthClient oAuthClient) : base(passport, "https://route.lgthinq.com:46030/v1", oAuthClient) { }

        public async Task<Gateway> GetGateway()
        {
            var httpRequestMessage = GetHttpRequestMessage(HttpMethod.Get, "/service/application/gateway-uri");
            return await ExecuteRequest<Gateway>(httpRequestMessage);
        }
    }
}
