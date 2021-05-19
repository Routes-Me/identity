using Encryption;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RoutesSecurity;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IdentitiesService.Abstraction;
using IdentitiesService.Helper.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.Common;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IdentitiesServiceContext _context;
        private readonly IHelperRepository _helper;
        private readonly IPasswordHasherRepository _passwordHasherRepository;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;
        EncryptionClass encryption = new EncryptionClass();

        public AccountRepository(IOptions<AppSettings> appSettings, IdentitiesServiceContext context, IHelperRepository helper, IPasswordHasherRepository passwordHasherRepository, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _helper = helper;
            _passwordHasherRepository = passwordHasherRepository;
            _dependencies = dependencies.Value;
        }

        public void AuthenticatePassword(EmailIdentities emailIdentities, string originalPassword)
        {
            if (!_passwordHasherRepository.Check(emailIdentities.Password, originalPassword).Verified)
                throw new ArgumentException(CommonMessage.IncorrectPassword);
        }

        private async Task<string> Validate(SigninDto signinModel) {
            if (string.IsNullOrEmpty(signinModel.Username))
                throw new ArgumentNullException(CommonMessage.MissingUserName);
            if (string.IsNullOrEmpty(signinModel.Password))
                throw new ArgumentNullException(CommonMessage.MissingPassword);

            string originalPassword = await PasswordDecryptionAsync(signinModel.Password);
            if (originalPassword == "Unauthorized Access")
                throw new ArgumentException(CommonMessage.IncorrectPassword);

            return originalPassword;
        }

        private List<RolesDto> GetIdentitiesRoles(Identities identity)
        {
            List<RolesDto> identityRoles = _context.IdentitiesRoles.Where(i => i.IdentityId == identity.IdentityId)
                    .Select(i => new RolesDto {
                        Application = i.Roles.Application.Name,
                        Privilege = i.Roles.Privilege.Name
                    }).ToList();

            if (identityRoles == null || identityRoles.Count == 0)
            {
                throw new ArgumentException(CommonMessage.IncorrectUserRole);
            }
            return identityRoles;
        }

        public async Task<AuthenticationResponse> AuthenticateUser(SigninDto signinDto, StringValues application)
        {
            string originalPassword = await Validate(signinDto);
            Identities identity = _context.Identities.Include(x => x.EmailIdentity).Where(x => x.EmailIdentity.Email == signinDto.Username).FirstOrDefault();
            if (identity == null)
                throw new ArgumentException(CommonMessage.IncorrectUsername);

            AuthenticatePassword(identity.EmailIdentity, originalPassword);
            List<RolesDto> identitiesRoles = GetIdentitiesRoles(identity);

            AccessTokenGenerator accessTokenGenerator = new AccessTokenGenerator()
            {
                UserId = Obfuscation.Encode(identity.UserId),
                Roles = identitiesRoles
            };
            string accessToken = _helper.GenerateAccessToken(accessTokenGenerator, application);
            string refreshToken = _helper.GenerateRefreshToken(accessToken);

            return new AuthenticationResponse()
            {
                identity = identity,
                accessToken = accessToken,
                refreshToken = refreshToken
            };
        }

        public TokenRenewalResponse RenewTokens(string refreshToken, string accessToken)
        {
            if (!_helper.validateTokens(refreshToken, accessToken))
                throw new AccessViolationException(CommonMessage.Unauthorized);
            return _helper.RenewTokens(refreshToken, accessToken);
        }

        public string GenerateRegistrationToken()
        {
            return _helper.GenerateRegistrationToken();
        }

        public dynamic RevokeRefreshToken(string refreshToken)
        {
            return _helper.RevokeRefreshToken(refreshToken);
        }

        public dynamic RevokeAccessToken(string accessToken)
        {
            return _helper.RevokeAccessToken(accessToken);
        }

        public async Task<string> PasswordDecryptionAsync(string Password)
        {
            if (encryption.IndexOfBSign(Password) != -1)
                return await encryption.DecodeAndDecrypt(Password, _appSettings.IVForDashboard, _appSettings.KeyForDashboard);
            else
                return await encryption.DecodeAndDecrypt(Password, _appSettings.IVForAndroid, _appSettings.KeyForAndroid);
        }
    }
}
