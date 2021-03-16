using System;

namespace IdentitiesService.Models.DBModels
{
    public partial class EmailIdentities
    {
        public int EmailIdentityId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int IdentityId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Identities Identity { get; set; }
    }
}
