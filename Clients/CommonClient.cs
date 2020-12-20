using EP94.LgSmartThinq.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public class CommonClient : ThinqApiClient
    {
        public CommonClient(Passport passport, string baseUrl, OAuthClient oAuthClient) : base(passport, baseUrl, oAuthClient)
        {
        }

        public async Task<RouteResponse> GetRoute()
        {
            using HttpRequestMessage httpRequestMessage = GetHttpRequestMessage(HttpMethod.Get, $"/route");
            return await ExecuteRequest<RouteResponse>(httpRequestMessage);
        }
    }
}
