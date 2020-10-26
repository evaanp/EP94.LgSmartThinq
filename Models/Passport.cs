using System;
using System.Collections.Generic;
using System.Text;

namespace EP94.LgSmartThinq.Models
{
    public class Passport
    {
        public OAuthToken Token { get; set; }
        public UserProfile UserProfile { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
    }
}
