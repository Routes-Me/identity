using System;

namespace IdentitiesService.Models.ResponseModel
{
    public class ApplicationsModel
    {
        public string ApplicationId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
