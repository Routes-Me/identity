using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Models.ResponseModel
{
    public class VerifyOTPModel
    {
        public string Phone { get; set; }
        public string Code { get; set; }
    }
}
