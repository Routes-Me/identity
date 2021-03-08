using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Models.ResponseModel
{
    public class EmailModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string RedirectUrl { get; set; }
    }
}
