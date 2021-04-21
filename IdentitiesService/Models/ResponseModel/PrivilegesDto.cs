using System;

namespace IdentitiesService.Models.ResponseModel
{
    public class PrivilegesDto
    {
        public string PrivilegeId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
