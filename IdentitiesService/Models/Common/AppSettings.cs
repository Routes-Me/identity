using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Models.Common
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string AccessSecretKey { get; set; }
        public string RefreshSecretKey { get; set; }
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
        public string SessionTokenIssuer { get; set; }
        public string Host { get; set; }
        public string IVForAndroid { get; set; }
        public string KeyForAndroid { get; set; }
        public string IVForDashboard { get; set; }
        public string KeyForDashboard { get; set; }
        public int Prime { get; set; }
        public int PrimeInverse { get; set; }
        public string RoutesAppUrl { get; set; }
    }
}
