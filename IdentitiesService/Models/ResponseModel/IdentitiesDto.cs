using System;
using System.Collections.Generic;

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

    public class UpdateIdentitiesDto
    {
        public string UserId { get; set; }
        public string IdentityId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public List<RolesModel> Roles { get; set; }
        public string InstitutionId { get; set; }
    }
}
