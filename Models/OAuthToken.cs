using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EP94.LgSmartThinq.Models
{
    public class OAuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("oauth2_backend_url")]
        public string OAuth2BackendUrl { get; set; }
    }
}
