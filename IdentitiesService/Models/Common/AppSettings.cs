﻿namespace IdentitiesService.Models.Common
{
    public class AppSettings
    {
        public string AccessSecretKey { get; set; }
        public string RefreshSecretKey { get; set; }
        public string SessionTokenIssuer { get; set; }
        public string RefreshTokenAudience { get; set; }
        public string Host { get; set; }
        public string IVForAndroid { get; set; }
        public string KeyForAndroid { get; set; }
        public string IVForDashboard { get; set; }
        public string KeyForDashboard { get; set; }
    }
}
