using System.Collections.Generic;

namespace IdentitiesService.Models.ResponseModel
{
    public class RegistrationDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public RolesDto Roles { get; set; }
        public string InstitutionId { get; set; }
    }
}
