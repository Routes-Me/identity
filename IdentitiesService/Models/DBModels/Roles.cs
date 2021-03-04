using System.Collections.Generic;

namespace IdentitiesService.Models.DBModels
{
    public partial class Roles
    {
        public int ApplicationId { get; set; }
        public int PrivilegeId { get; set; }

        public virtual Applications Application { get; set; }
        public virtual Privileges Privilege { get; set; }
        public virtual ICollection<IdentitiesRoles> IdentitiesRoles { get; set; }
    }
}
