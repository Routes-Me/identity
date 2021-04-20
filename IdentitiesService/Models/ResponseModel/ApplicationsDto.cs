using System;

namespace IdentitiesService.Models.ResponseModel
{
    public class ApplicationsDto
    {
        public string ApplicationId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
