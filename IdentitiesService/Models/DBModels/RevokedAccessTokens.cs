using System;

namespace IdentitiesService.Models.DBModels
{
    public partial class RevokedAccessTokens
    {
        public int RevokedAccessTokenId { get; set; }
        public int UserId { get; set; }
        public string AccessTokenReference { get; set; }
        public DateTime ExpiryAt { get; set; }
        public DateTime RevokedAt { get; set; }
    }
}
