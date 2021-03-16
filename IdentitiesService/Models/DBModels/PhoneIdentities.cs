using System;

namespace IdentitiesService.Models.DBModels
{
    public partial class PhoneIdentities
    {
        public int PhoneIdentityId { get; set; }
        public string PhoneNumber { get; set; }
        public int IdentityId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Identities Identity { get; set; }
    }
}
