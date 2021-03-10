﻿using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;
using IdentitiesService.Models.DBModels;

namespace IdentitiesService.Abstraction
{
    public interface IAccountRepository
    {
        Task<dynamic> SignUp(RegistrationDto model);
        Task<AuthenticationResponse> AuthenticateUser(SigninModel signinModel, StringValues application);
        TokenRenewalResponse RenewTokens(string refreshToken, string accessToken);
        dynamic RevokeRefreshToken(string refreshToken);
        dynamic RevokeAccessToken(string accessToken);
        Task<dynamic> ChangePassword(ChangePasswordModel model);
        Task<dynamic> ForgotPassword(string email);
    }
}
