using Microsoft.Extensions.Primitives;
using IdentitiesService.Models.Common;
using IdentitiesService.Models;

namespace IdentitiesService.Helper.Abstraction
{
    public interface IHelperRepository
    {
        string GenerateAccessToken(AccessTokenGenerator accessTokenGenerator, StringValues application);
        string GenerateRefreshToken(string accessToken);
        bool validateTokens(string refreshToken, string accessToken);
        dynamic VerifyToken(string token);
        TokenRenewalResponse RenewTokens(string refreshToken, string accessToken);
        dynamic RevokeRefreshToken(string refreshToken);
        dynamic RevokeAccessToken(string accessToken);
        void RemoveExpiredTokens();
    }
}
