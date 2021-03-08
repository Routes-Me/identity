using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Models.ResponseModel
{
    public class IdentitiesDto
    {
        public string UserId { get; set; }
        public string IdentityId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<RolesModel> Roles { get; set; }
    }

    public class LoginUser
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public List<IdentityRoleForToken> Roles { get; set; }
        public bool isOfficer { get; set; }
        public string OfficerId { get; set; }
        
    }
}
