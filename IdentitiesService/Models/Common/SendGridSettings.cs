using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Models.Common
{
    public class SendGridSettings
    {
        public string APIKey { get; set; }
        public string From { get; set; }
        public string Name { get; set; }
        public string SubjectEmailVerify { get; set; }
        public string SubjectForgotPassword { get; set; }
    }
}
