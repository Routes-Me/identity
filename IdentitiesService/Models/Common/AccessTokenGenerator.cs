using System.Collections.Generic;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Models.Common
{
    public class AccessTokenGenerator
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public List<IdentityRoleForToken> Roles { get; set; }
        public string InstitutionId { get; set; }
    }

}
