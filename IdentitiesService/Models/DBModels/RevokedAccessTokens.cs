using System;

namespace IdentitiesService.Models.DBModels
{
    public partial class RevokedAccessTokens
    {
        public int RevokedAccessTokenId { get; set; }
        public int IdentityId { get; set; }
        public string AccessTokenReference { get; set; }
        public DateTime ExpiryAt { get; set; }
        public DateTime RevokedAt { get; set; }

        public virtual Identities Identity { get; set; }
    }
}
