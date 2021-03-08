using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Models.ResponseModel
{
    public class ApplicationsModel
    {
        public string ApplicationId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
