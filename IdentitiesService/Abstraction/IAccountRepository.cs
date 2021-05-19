using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using IdentitiesService.Models;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IAccountRepository
    {
        Task<AuthenticationResponse> AuthenticateUser(SigninDto signinDto, StringValues application);
        TokenRenewalResponse RenewTokens(string refreshToken, string accessToken);
        dynamic RevokeRefreshToken(string refreshToken);
        dynamic RevokeAccessToken(string accessToken);
        Task<string> PasswordDecryptionAsync(string Password);
        string GenerateRegistrationToken();
    }
}
