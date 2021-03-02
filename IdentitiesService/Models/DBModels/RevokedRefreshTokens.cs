using System;

namespace IdentitiesService.Models.DBModels
{
    public partial class RevokedRefreshTokens
    {
        public int RevokedRefreshTokenId { get; set; }
        public int IdentityId { get; set; }
        public string RefreshTokenReference { get; set; }
        public DateTime ExpiryAt { get; set; }
        public DateTime RevokedAt { get; set; }
    }
}
