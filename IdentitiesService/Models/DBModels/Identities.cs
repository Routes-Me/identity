using System;
using System.Collections.Generic;

namespace IdentitiesService.Models.DBModels
{
    public partial class Identities
    {
        public int IdentityId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<IdentitiesRoles> IdentitiesRoles { get; set; }
        public virtual ICollection<PhoneIdentities> PhoneIdentities { get; set; }
        public virtual ICollection<RevokedAccessTokens> RevokedAccessTokens { get; set; }
        public virtual ICollection<RevokedRefreshTokens> RevokedRefreshTokens { get; set; }
        public virtual EmailIdentities EmailIdentity { get; set; }
    }
}
