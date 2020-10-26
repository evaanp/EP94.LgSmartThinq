﻿using EP94.LgSmartThinq.Models;
using EP94.LgSmartThinq.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public abstract class ThinqApiClient
    {
        private string _baseUrl { get; }
        private Passport _passport;
        public ThinqApiClient(Passport passport, string baseUrl)
        {
            _passport = passport;
            _baseUrl = baseUrl;
        }

        protected HttpRequestMessage GetHttpRequestMessage(HttpMethod httpMethod, string relativeUrl)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, $"{_baseUrl}{relativeUrl}");
            requestMessage.Headers.Add("x-client-id", Constants.CLIENT_ID);
            requestMessage.Headers.Add("x-country-code", _passport.Country);
            requestMessage.Headers.Add("x-language-code", _passport.Language);
            requestMessage.Headers.Add("x-message-id", new Regex("=*$").Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), ""));
            requestMessage.Headers.Add("x-api-key", Constants.API_KEY);
            requestMessage.Headers.Add("x-service-code", Constants.SERVICE_CODE);
            requestMessage.Headers.Add("x-service-phase", Constants.SERVICE_PHASE);
            requestMessage.Headers.Add("x-thinq-app-level", Constants.APP_LEVEL);
            requestMessage.Headers.Add("x-thinq-app-os", Constants.APP_OS);
            requestMessage.Headers.Add("x-thinq-app-type", Constants.APP_TYPE);
            requestMessage.Headers.Add("x-thinq-app-ver", Constants.APP_VERSION);
            requestMessage.Headers.Add("client_id", Constants.CLIENT_ID);
            requestMessage.Headers.Add("country_code", _passport.Country);
            requestMessage.Headers.Add("language_code", _passport.Language);
            requestMessage.Headers.Add("x-emp-token", _passport.Token.AccessToken);
            requestMessage.Headers.Add("x-user-no", _passport.UserProfile.UserNo);
            requestMessage.Headers.Add("Authorization", $"Bearer {_passport.Token.AccessToken}");
            return requestMessage;
        }

        protected async Task<bool> ExecuteRequest(HttpRequestMessage httpRequestMessage)
        {
            using HttpClient httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            string responseString = await response.Content.ReadAsStringAsync();
            JObject jObject = JsonConvert.DeserializeObject<JObject>(responseString);
            return jObject["resultCode"].Value<int>() == ErrorCodes.OK;
        }

        protected async Task<T> ExecuteRequest<T>(HttpRequestMessage httpRequestMessage, string path = null)
        {
            using HttpClient httpClient = new HttpClient();
            using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            string responseString = await response.Content.ReadAsStringAsync();
            JObject jObject = JsonConvert.DeserializeObject<JObject>(responseString);
            if (jObject["resultCode"].Value<int>() == ErrorCodes.OK)
            {
                if (path == null)
                    return jObject["result"].ToObject<T>();
                else
                {
                    JToken current = jObject["result"];
                    foreach (string pathParth in path.Split("."))
                    {
                        current = current[pathParth];
                    }
                    return current.ToObject<T>();
                }
            }
            else
                return default;
        }
    }
}
