using System;
using System.Collections.Generic;
using System.Text;

namespace EP94.LgSmartThinq.Utils
{
    public static class Constants
    {
        public static string CLIENT_ID = Guid.NewGuid().ToString();
        public static string API_KEY = "VGhpblEyLjAgU0VSVklDRQ==";
        public static string SERVICE_CODE = "SVC202";
        public static string SERVICE_PHASE = "OP";
        public static string APP_LEVEL = "PRD";
        public static string APP_OS = "ANDROID";
        public static string APP_TYPE = "NUTS";
        public static string APP_VERSION = "3.0.1700";
        public static string DIVISION = "ha";
        public static string OAUTH_REDIRECT_URI = "https://kr.m.lgaccount.com/login/iabClose";
        public static string OAUTH_TIMESTAMP_FORMAT = "ddd, dd MMM yyyy HH:mm:ss +0000";
        public static string OAUTH_SECRET = "c053c2a6ddeb7ad97cb0eed0dcb31cf8";
        public static string LGE_APP_KEY = "LGAO221A02";
        public static string THIRD_PARTY_LOGINS = "GGL,AMZ,FBK";

        public static string AWS_IOTT_CA_CERT_URL = "https://www.websecurity.digicert.com/content/dam/websitesecurity/digitalassets/desktop/pdfs/roots/VeriSign-Class%203-Public-Primary-Certification-Authority-G5.pem";
        public static string AWS_IOTT_ALPN_PROTOCOL = "x-amzn-mqtt-ca";
        //public static string API_BASE_URL = "https://route.lgthinq.com:46030/v1/";
        public static string LOGIN_BASE_URL = "https://{0}.m.lgaccount.com";
        public static string API_BASE_URL = "https://gb.lgeapi.com";
    }
}
