using System.Collections.Generic;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Models.Common
{
    public class AccessTokenGenerator
    {
        public string UserId { get; set; }
        public List<RolesDto> Roles { get; set; }
    }
}
