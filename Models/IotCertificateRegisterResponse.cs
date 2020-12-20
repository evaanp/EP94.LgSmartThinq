using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EP94.LgSmartThinq.Models
{
    public class IotCertificateRegisterResponse
    {
        public X509Certificate2 CertificatePem { get; set; }
        public List<string> Subscriptions { get; set; }
    }
}
