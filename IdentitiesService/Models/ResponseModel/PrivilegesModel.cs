using System;

namespace IdentitiesService.Models.ResponseModel
{
    public class PrivilegesModel
    {
        public string PrivilegeId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
