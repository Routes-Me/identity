﻿using Encryption;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using Obfuscation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
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

        public async Task<dynamic> SignUp(RegistrationModel model)
        {
            UsersResponse response = new UsersResponse();
            List<RolesModel> roles = new List<RolesModel>();
            try
            {
                int institutionIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.InstitutionId), _appSettings.PrimeInverse);
                string originalPassword = string.Empty;
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                if (model.Roles.Count == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserRoleRequired, StatusCodes.Status400BadRequest);

                foreach (var role in model.Roles)
                {
                    var userRole = _context.Roles.Where(x => x.ApplicationId == ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse)
                    && x.PrivilegeId == ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (userRole == null)
                    {
                        return ReturnResponse.ErrorResponse(CommonMessage.UserRoleNotFound, StatusCodes.Status404NotFound);
                    }
                    else
                    {
                        RolesModel rolesModel = new RolesModel();
                        rolesModel.ApplicationId = ObfuscationClass.EncodeId(userRole.ApplicationId, _appSettings.Prime).ToString();
                        rolesModel.PrivilegeId = ObfuscationClass.EncodeId(userRole.PrivilegeId, _appSettings.Prime).ToString();
                        roles.Add(rolesModel);
                    }
                }

                if (string.IsNullOrEmpty(model.PhoneNumber) && string.IsNullOrEmpty(model.Email))
                    return ReturnResponse.ErrorResponse(CommonMessage.EmailPhoneRequired, StatusCodes.Status400BadRequest);

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var phones = _context.PhoneIdentities.Where(x => x.PhoneNumber == model.PhoneNumber).FirstOrDefault();
                    if (phones != null)
                        return ReturnResponse.ErrorResponse(CommonMessage.PhoneExist, StatusCodes.Status409Conflict);
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    var userEmail = _context.EmailIdentities.Where(x => x.Email == model.Email).FirstOrDefault();
                    if (userEmail != null)
                        return ReturnResponse.ErrorResponse(CommonMessage.EmailExist, StatusCodes.Status409Conflict);
                }

                if (!string.IsNullOrEmpty(model.Password))
                {
                    originalPassword = await PasswordDecryptionAsync(model.Password);
                    if (originalPassword == "Unauthorized Access")
                        return ReturnResponse.ErrorResponse(CommonMessage.IncorrectPassword, StatusCodes.Status400BadRequest);
                }

                Identities identity = new Identities()
                {
                    CreatedAt = DateTime.Now,
                };
                _context.Identities.Add(identity);
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    PhoneIdentities phone = new PhoneIdentities()
                    {
                        IdentityId = identity.IdentityId,
                        CreatedAt = DateTime.Now,
                        PhoneNumber = model.PhoneNumber,
                    };
                    _context.PhoneIdentities.Add(phone);
                    _context.SaveChanges();
                }

                foreach (var role in roles)
                {
                    IdentitiesRoles usersroles = new IdentitiesRoles()
                    {
                        IdentityId = identity.UserId,
                        ApplicationId = ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse),
                        PrivilegeId = ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)
                    };
                    _context.IdentitiesRoles.Add(usersroles);
                }
                _context.SaveChanges();

                if (institutionIdDecrypted != 0)
                {
                    DriversModel driver = new DriversModel()
                    {
                        InstitutionId = model.InstitutionId,
                        UserId = ObfuscationClass.EncodeId(identity.UserId, _appSettings.Prime).ToString()
                    };

                    var client = new RestClient(_appSettings.Host + _dependencies.VehicleUrl);
                    var request = new RestRequest(Method.POST);
                    string jsonToSend = JsonConvert.SerializeObject(driver);
                    request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    IRestResponse institutionResponse = client.Execute(request);
                    if (institutionResponse.StatusCode != HttpStatusCode.Created)
                    {
                        response.status = false;
                        response.message = CommonMessage.UnableToInsertUserIntoDriver + institutionResponse.Content;
                        response.statusCode = StatusCodes.Status500InternalServerError;
                        return response;
                    }
                }
                response.status = true;
                response.message = CommonMessage.UserInsert;
                response.statusCode = StatusCodes.Status201Created;
                response.Email = model.Email;
                response.UserId = ObfuscationClass.EncodeId(identity.UserId, _appSettings.Prime).ToString();
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public void Authenticate(EmailIdentities emailIdentities, string originalPassword)
        {
            if (emailIdentities == null || !_passwordHasherRepository.Check(emailIdentities.Password, originalPassword).Verified)
            {
                throw new Exception(CommonMessage.UserNotExist);
            }
        }

        private async Task<string> Validate(SigninModel model) {
            if (String.IsNullOrEmpty(model.Username) || String.IsNullOrEmpty(model.Password))
            {
                throw new Exception(CommonMessage.IncorrectUser);
            }
            string originalPassword = await PasswordDecryptionAsync(model.Password);
            if (originalPassword == "Unauthorized Access")
            {
                throw new Exception(CommonMessage.IncorrectPassword);
            }
            return originalPassword;
        }

        private List<IdentityRoleForToken> GetIdentitiesRoles(Identities identities)
        {
            List<IdentityRoleForToken> identityRoles = (from identitiesRoles in _context.IdentitiesRoles
                                  join roles in _context.Roles on new { x = identitiesRoles.PrivilegeId, y = identitiesRoles.ApplicationId } equals new { x = roles.PrivilegeId, y = roles.ApplicationId }
                                  where identitiesRoles.IdentityId == identities.IdentityId
                                  select new IdentityRoleForToken
                                  {
                                      Application = roles.Application.Name,
                                      Privilege = roles.Privilege.Name,
                                  }).ToList();

            if (identityRoles == null || identityRoles.Count == 0)
            {
                throw new Exception(CommonMessage.IncorrectUserRole);
            }

            return identityRoles;
        }

        private string GetUsersInstitutions(Identities identities)
        {
            var client = new RestClient(_appSettings.Host + _dependencies.InstitutionsUrl + ObfuscationClass.EncodeId(identities.IdentityId, _appSettings.Prime).ToString());
            var request = new RestRequest(Method.GET);
            IRestResponse driverResponse = client.Execute(request);
            if (driverResponse.StatusCode != HttpStatusCode.OK)
            {
                return string.Empty;
            }
            var institutionData = JsonConvert.DeserializeObject<InstitutionResponse>(driverResponse.Content);
            return String.Join(",", institutionData.data.Select(x => x.InstitutionId));
        }

        public async Task<AuthenticationResponse> AuthenticateUser(SigninModel signinModel, StringValues application)
        {
            string originalPassword = string.Empty;

            String username = signinModel.Username;
            String password = signinModel.Password;

            try
            {
                originalPassword = await Validate(signinModel);
                Identities identity = _context.Identities.Include(x => x.EmailIdentity).Where(x => x.EmailIdentity.Email == signinModel.Username).FirstOrDefault();
                Authenticate(identity.EmailIdentity, originalPassword);
                List<IdentityRoleForToken> identitiesRoles = GetIdentitiesRoles(identity);
                string institutionIds = GetUsersInstitutions(identity);

                AccessTokenGenerator accessTokenGenerator = new AccessTokenGenerator()
                {
                    UserId = ObfuscationClass.EncodeId(identity.UserId, _appSettings.Prime).ToString(),
                    Roles = identitiesRoles,
                    InstitutionId = institutionIds
                };
                string accessToken = _helper.GenerateAccessToken(accessTokenGenerator, application);

                string refreshToken = _helper.GenerateRefreshToken(accessToken);

                return new AuthenticationResponse()
                {
                    user = identity,
                    accessToken = accessToken,
                    refreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public TokenRenewalResponse RenewTokens(string refreshToken, string accessToken)
        {
            if (!_helper.validateTokens(refreshToken, accessToken))
                throw new AccessViolationException(CommonMessage.Unauthorized);
            return _helper.RenewTokens(refreshToken, accessToken);
        }

        public async Task<dynamic> ChangePassword(ChangePasswordModel model)
        {
            UsersResponse response = new UsersResponse();
            string originalPassword = string.Empty;
            try
            {
                int UserId = ObfuscationClass.DecodeId(Convert.ToInt32(model.UserId), _appSettings.PrimeInverse);
                Identities identity = new Identities();
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                identity = _context.Identities.Where(x => x.UserId == UserId).FirstOrDefault();
                if (identity == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.IdentityNotFound, StatusCodes.Status404NotFound);

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    originalPassword = await PasswordDecryptionAsync(model.NewPassword);
                    if (originalPassword == "Unauthorized Access")
                        return ReturnResponse.ErrorResponse(CommonMessage.IncorrectPassword, StatusCodes.Status400BadRequest);
                }

                identity.EmailIdentity.Password = _passwordHasherRepository.Hash(originalPassword);
                _context.Identities.Update(identity);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.ChangePasswordSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic RevokeRefreshToken(string refreshToken)
        {
            return _helper.RevokeRefreshToken(refreshToken);
        }

        public dynamic RevokeAccessToken(string accessToken)
        {
            return _helper.RevokeAccessToken(accessToken);
        }

        public async Task<dynamic> ForgotPassword(string email)
        {
            EmailResponse response = new EmailResponse();
            try
            {
                if (string.IsNullOrEmpty(email))
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var identity = _context.Identities.Where(x => x.EmailIdentity.Email.ToLower() == email.ToLower()).FirstOrDefault();
                if (identity == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.EmailNotFound, StatusCodes.Status404NotFound);

                var res = await _helper.VerifyEmail(email, identity);
                if (res.StatusCode != HttpStatusCode.Accepted)
                    return ReturnResponse.ErrorResponse(CommonMessage.ForgotPasswordFailed, StatusCodes.Status500InternalServerError);

                return ReturnResponse.SuccessResponse(CommonMessage.ForgotPasswordSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
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
