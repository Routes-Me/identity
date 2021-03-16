using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using IdentitiesService.Models.Common;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models;

namespace IdentitiesService.Helper.Abstraction
{
    public interface IHelperRepository
    {
        string GenerateAccessToken(AccessTokenGenerator accessTokenGenerator, StringValues application);
        string GenerateRefreshToken(string accessToken);
        bool validateTokens(string refreshToken, string accessToken);
        dynamic VerifyToken(string token);
        Task<SendGrid.Response> VerifyEmail(string email, Identities identity);
        Task<SendGrid.Response> SendConfirmationEmail(int userId, string email, string siteUrl);
        TokenRenewalResponse RenewTokens(string refreshToken, string accessToken);
        dynamic RevokeRefreshToken(string refreshToken);
        dynamic RevokeAccessToken(string accessToken);
        void RemoveExpiredTokens();
    }
}
