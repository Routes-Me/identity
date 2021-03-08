using System;
using System.Collections.Generic;

namespace IdentitiesService.Models.DBModels
{
    public partial class Privileges
    {
        public Privileges()
        {
            Roles = new HashSet<Roles>();
        }

        public int PrivilegeId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Roles> Roles { get; set; }
    }
}
