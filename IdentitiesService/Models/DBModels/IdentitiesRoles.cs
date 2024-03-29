﻿namespace IdentitiesService.Models.DBModels
{
    public partial class IdentitiesRoles
    {
        public int IdentityId { get; set; }
        public int ApplicationId { get; set; }
        public int PrivilegeId { get; set; }

        public virtual Roles Roles { get; set; }
        public virtual Identities Identity { get; set; }
    }
}
