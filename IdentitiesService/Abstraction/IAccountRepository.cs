using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using IdentitiesService.Models;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IAccountRepository
    {
        Task<Identities> PostIdentity(RegistrationDto registrationDto);
        Task<AuthenticationResponse> AuthenticateUser(SigninDto signinDto, StringValues application);
        TokenRenewalResponse RenewTokens(string refreshToken, string accessToken);
        dynamic RevokeRefreshToken(string refreshToken);
        dynamic RevokeAccessToken(string accessToken);
        Task<dynamic> ChangePassword(ChangePasswordModel model);
        Task<dynamic> ForgotPassword(string email);
    }
}
