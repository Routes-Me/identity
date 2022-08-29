using Microsoft.Extensions.Primitives;
using IdentitiesService.Models.Common;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Helper.Abstraction
{
    public interface IHelperRepository
    {
        string GenerateAccessToken(AccessTokenGenerator accessTokenGenerator, StringValues application);
        string GenerateRefreshToken(string accessToken);
        bool validateTokens(string refreshToken, string accessToken);
        dynamic VerifyToken(string token);
        TokenRenewalResponse RenewTokens(string refreshToken, string accessToken);
        string GenerateInvitationToken(InvitationTokenGenerationDto invitationTokenGenerationDto);
        dynamic RevokeRefreshToken(string refreshToken);
        dynamic RevokeAccessToken(string accessToken);
        void RemoveExpiredTokens();
    }
}
