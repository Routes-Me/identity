using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RoutesSecurity;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;
using IdentitiesService.Helper.Abstraction;
using IdentitiesService.Models.Common;
using IdentitiesService.Models;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Helper.Repository
{
    public class HelperRepository : IHelperRepository
    {
        private readonly AppSettings _appSettings;
        private readonly SendGridSettings _sendGridSettings;
        private readonly IdentitiesServiceContext _context;
        private readonly Dependencies _dependencies;

        public HelperRepository(IOptions<AppSettings> appSettings, IOptions<Dependencies> dependencies, IOptions<SendGridSettings> sendGridSettings, IdentitiesServiceContext context)
        {
            _appSettings = appSettings.Value;
            _dependencies = dependencies.Value;
            _sendGridSettings = sendGridSettings.Value;
            _context = context;
        }

        public string GenerateAccessToken(AccessTokenGenerator accessTokenGenerator, StringValues application)
        {
            if (accessTokenGenerator == null)
                throw new ArgumentNullException(CommonMessage.TokenDataNull);

            var key = Encoding.UTF8.GetBytes(_appSettings.AccessSecretKey);
            string tokenId = JsonConvert.DeserializeObject<IdentifierResponse>(GetAPI(_dependencies.GetIdentifierUrl).Content).Identifier.ToString();
            string officerId = JsonConvert.DeserializeObject<OfficerResponse>(GetAPI(_dependencies.GetOfficerIdUrl, "userId=" + accessTokenGenerator.UserId).Content).OfficerId.ToString();
            ExtrasDto extrasDto = new ExtrasDto { OfficerId = officerId };

            var claimsData = new Claim[]
            {
                new Claim("sub", accessTokenGenerator.UserId.ToString()),
                new Claim("rol", JsonConvert.SerializeObject(accessTokenGenerator.Roles)),
                new Claim("ref", tokenId),
                application.ToString().ToLower() == "dashboard" ? new Claim("ext", JsonConvert.SerializeObject(extrasDto)) : null
            };

            var tokenString = new JwtSecurityToken(
                                issuer: _appSettings.SessionTokenIssuer,
                                audience: GetAudience(application),
                                expires: application.ToString().ToLower() == "screen" ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddMinutes(15),
                                claims: claimsData,
                                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                );
            return new JwtSecurityTokenHandler().WriteToken(tokenString);
        }

        public string GenerateRefreshToken(string accessToken)
        {
            JwtSecurityToken tokenData = new JwtSecurityTokenHandler().ReadToken(accessToken) as JwtSecurityToken;
            var key = Encoding.UTF8.GetBytes(_appSettings.RefreshSecretKey);
            var tokenId = JsonConvert.DeserializeObject<IdentifierResponse>(GetAPI(_dependencies.GetIdentifierUrl).Content).Identifier.ToString();
            var claimsData = new Claim[]
            {
                new Claim("sub", tokenData.Claims.First(c => c.Type == "sub").Value),
                new Claim("ref", tokenId),
                new Claim("stm", EncodeSignature(accessToken.Split('.').Last()))
            };

            var tokenString = new JwtSecurityToken(
                issuer: _appSettings.SessionTokenIssuer,
                audience: _appSettings.RefreshTokenAudience,
                expires: tokenData.Audiences.FirstOrDefault() == "https://screen.routesme.com" ? DateTime.UtcNow.AddMonths(6) : DateTime.UtcNow.AddMonths(3),
                claims: claimsData,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );
            return new JwtSecurityTokenHandler().WriteToken(tokenString);
        }

        public bool validateTokens(string refreshToken, string accessToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(refreshToken) || !tokenHandler.CanReadToken(accessToken))
                return false;

            JwtSecurityToken accessTokenData = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;
            JwtSecurityToken refreshTokenData = tokenHandler.ReadToken(refreshToken) as JwtSecurityToken;

            if (accessTokenData.ValidTo > DateTime.UtcNow)
                throw new SecurityTokenExpiredException(CommonMessage.Forbidden);

            string stamp = refreshTokenData.Claims.First(c => c.Type == "stm").Value;
            string decodedSignature = DecodeSignature(stamp);

            string accessTokenSignature = accessToken.Split('.').Last();

            if (accessTokenSignature.Equals(decodedSignature) && refreshTokenData.ValidTo > DateTime.UtcNow && refreshTokenData.Issuer.Equals(_appSettings.SessionTokenIssuer))
                return true;
            return false;
        }

        public TokenRenewalResponse RenewTokens(string refreshToken, string accessToken)
        {
            JwtSecurityToken tokenData = new JwtSecurityTokenHandler().ReadToken(accessToken) as JwtSecurityToken;

            var key = Encoding.UTF8.GetBytes(_appSettings.AccessSecretKey);
            string newAccessToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: _appSettings.SessionTokenIssuer,
                audience: tokenData.Audiences.FirstOrDefault(),
                expires: tokenData.Audiences.FirstOrDefault() == "https://screen.routesme.com" ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddMinutes(15),
                claims: tokenData.Claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            ));

            var revokedToken = RevokeRefreshToken(refreshToken);
            _context.RevokedRefreshTokens.Add(revokedToken);
            _context.SaveChanges();
            string newRefreshToken = GenerateRefreshToken(newAccessToken);

            return new TokenRenewalResponse()
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            };
        }

        public dynamic RevokeRefreshToken(string refreshToken)
        {
            JwtSecurityToken refreshTokenData = VerifyToken(refreshToken);

            string tokenreference = refreshTokenData.Claims.First(c => c.Type == "ref").Value;
            if (_context.RevokedRefreshTokens.Where(r => r.RefreshTokenReference == tokenreference).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.TokenAlreadyRevoked);

            int userId = Obfuscation.Decode(refreshTokenData.Claims.First(c => c.Type == "sub").Value);
            Identities identity = _context.Identities.Where(i => i.UserId == userId).FirstOrDefault();
            if (identity == null)
                throw new Exception(CommonMessage.InvalidIdentityToken);

            RevokedRefreshTokens revokedToken = new RevokedRefreshTokens
            {
                RefreshTokenReference =  refreshTokenData.Claims.First(c => c.Type == "ref").Value,
                IdentityId = identity.IdentityId,
                ExpiryAt = refreshTokenData.ValidTo,
                RevokedAt = DateTime.Now
            };
            return revokedToken;
        }

        public dynamic RevokeAccessToken(string accessToken)
        {
            JwtSecurityToken accessTokenData = VerifyToken(accessToken);

            string tokenreference = accessTokenData.Claims.First(c => c.Type == "ref").Value;
            if (_context.RevokedAccessTokens.Where(r => r.AccessTokenReference == tokenreference).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.TokenAlreadyRevoked);

            int userId = Obfuscation.Decode(accessTokenData.Claims.First(c => c.Type == "sub").Value);
            Identities identity = _context.Identities.Where(i => i.UserId == userId).FirstOrDefault();
            if (identity == null)
                throw new Exception(CommonMessage.InvalidIdentityToken);

            RevokedAccessTokens revokedToken = new RevokedAccessTokens
            {
                AccessTokenReference = accessTokenData.Claims.First(c => c.Type == "ref").Value,
                IdentityId = Obfuscation.Decode(accessTokenData.Claims.First(c => c.Type == "sub").Value),
                ExpiryAt = accessTokenData.ValidTo,
                RevokedAt = DateTime.Now
            };
            return revokedToken;
        }

        public void RemoveExpiredTokens()
        {
            List<RevokedAccessTokens> expiredAccessTokens = _context.RevokedAccessTokens.Where(e => e.ExpiryAt < DateTime.Now).ToList();
            List<RevokedRefreshTokens> expiredRefreshTokens = _context.RevokedRefreshTokens.Where(e => e.ExpiryAt < DateTime.Now).ToList();
            _context.RevokedAccessTokens.RemoveRange(expiredAccessTokens);
            _context.RevokedRefreshTokens.RemoveRange(expiredRefreshTokens);
            _context.SaveChangesAsync();
        }

        public async Task<SendGrid.Response> SendConfirmationEmail(int userId, string email, string siteUrl)
        {
            try
            {
                var client = new SendGridClient(_sendGridSettings.APIKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                    Subject = _sendGridSettings.SubjectEmailVerify,
                    HtmlContent = "Please <a href='" + siteUrl + "?id=" + userId + "'>click here</a> to verify."
                };
                msg.AddTo(new EmailAddress(email));
                msg.SetClickTracking(false, false);
                return await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SendGrid.Response> VerifyEmail(string email, Identities identity)
        {
            try
            {
                bool IsRoutesApp = false;
                foreach (var item in identity.IdentitiesRoles)
                {
                    var Application = _context.Applications.Where(x => x.ApplicationId == item.ApplicationId).FirstOrDefault();
                    if (Application != null)
                    {
                        if (Application.ToString().ToLower() == "userapp")
                        {
                            IsRoutesApp = true;
                        }
                    }
                }
                string UserId = Obfuscation.Encode(identity.UserId);
                var client = new SendGridClient(_sendGridSettings.APIKey);
                if (IsRoutesApp == true)
                {
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                        Subject = _sendGridSettings.SubjectForgotPassword,
                        HtmlContent = "<div style='background-color: white;max-width: 414px;margin: 0 auto;padding: 20px;min-width: 600px; line-height: 1.5; font-size: 18px;'> <h3>Hi, </h3><div>You recently requested to reset your password for your RoutesApp account. Click the button below to reset it.</div><div style='border-radius: 10px;background-color: #1a82e2;text-align: center;width: max-content;margin: auto;margin-top: 30px;margin-bottom: 30px;'><a href=" + _appSettings.RoutesAppUrl + UserId + " target='_blank' style='display: inline-block; padding: 16px 36px; font-family: &#39Source Sans Pro&#39, Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;'>Reset your password</a></div><div> If you did not request a password reset, please ignore this email or reply to let us know. This password reset is only valid for the next 30 minutes.</div><br/><div>Thanks,</div><div>RoutesApp Team</div><br/><div>If you asre having trouble clicking the password reset button, copy and paste the URL below into your web browser.</div><div><a href=" + _appSettings.RoutesAppUrl + UserId + "  target='_blank'>" + _appSettings.RoutesAppUrl + UserId + " </a></div></div>"
                    };
                    msg.AddTo(new EmailAddress(email));
                    msg.SetClickTracking(false, false);
                    return await client.SendEmailAsync(msg);
                }
                else
                {
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                        Subject = _sendGridSettings.SubjectForgotPassword,
                        HtmlContent = "<div style='background-color: white;max-width: 414px;margin: 0 auto;padding: 20px;min-width: 600px; line-height: 1.5; font-size: 18px;'> <h3>Hi, </h3><div>You recently requested to reset your password for your RoutesApp account. Click the button below to reset it.</div><div style='border-radius: 10px;background-color: #1a82e2;text-align: center;width: max-content;margin: auto;margin-top: 30px;margin-bottom: 30px;'><a href=" + _appSettings.RoutesAppUrl + UserId + "  target='_blank' style='display: inline-block; padding: 16px 36px; font-family: &#39Source Sans Pro&#39, Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;'>Reset your password</a></div><div> If you did not request a password reset, please ignore this email or reply to let us know. This password reset is only valid for the next 30 minutes.</div><br/><div>Thanks,</div><div>RoutesApp Team</div><br/><div>If you asre having trouble clicking the password reset button, copy and paste the URL below into your web browser.</div><div><a href=" + _appSettings.RoutesAppUrl + UserId + "  target='_blank'>" + _appSettings.RoutesAppUrl + UserId + " </a></div></div>"
                    };
                    msg.AddTo(new EmailAddress(email));
                    msg.SetClickTracking(false, false);
                    return await client.SendEmailAsync(msg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public dynamic VerifyToken(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            if (string.IsNullOrEmpty(token) || !tokenHandler.CanReadToken(token))
                throw new ArgumentNullException(CommonMessage.InvalidData);

            JwtSecurityToken tokenData = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (tokenData.ValidTo < DateTime.UtcNow)
                throw new SecurityTokenExpiredException(CommonMessage.TokenExpired);

            return tokenData;
        }

        private static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes(plainText));
        }

        private static string Base64Decode(string encodedText)
        {
            return Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(encodedText));
        }

        private static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string EncodeSignature(string signature)
        {
            string encodedSignature =  Base64Encode(signature);
            int index = encodedSignature[encodedSignature.Length - 3] % 7;
            if (index == 0)
                index = 7;
            return encodedSignature.Insert(index, GenerateRandomString(index));
        }

        private static string DecodeSignature(string signature)
        {
            int index = signature[signature.Length - 3] % 7;
            if (index == 0)
                index = 7;
            var signatureWithoutRedunduncy = signature.Remove(index, index);
            return Base64Decode(signatureWithoutRedunduncy);
        }

        private string GetAudience(StringValues application)
        {
            switch (application.ToString().ToLower())
            {
                case "dashboard": return "https://dashboard.routesme.com";
                case "app": return "https://app.routesme.com";
                case "screen": return "https://screen.routesme.com";
                default: return CommonMessage.UnknownApplication;
            }
        }

        private async Task PostAPI(string url, dynamic objectToSend)
        {
            var client = new RestClient(_appSettings.Host + url);
            var request = new RestRequest(Method.POST);
            string jsonToSend = JsonConvert.SerializeObject(objectToSend);
            request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = client.Execute(request);
            await Task.CompletedTask;
        }

        private dynamic GetAPI(string url, string query = "")
        {
            UriBuilder uriBuilder = new UriBuilder(_appSettings.Host + url);
            uriBuilder = AppendQueryToUrl(uriBuilder, query);
            var client = new RestClient(uriBuilder.Uri);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == 0)
                throw new HttpListenerException(400, CommonMessage.ConnectionFailure);

            if (!response.IsSuccessful)
                throw new HttpListenerException((int)response.StatusCode, response.Content);

            return response;
        }

        public UriBuilder AppendQueryToUrl(UriBuilder baseUri, string queryToAppend)
        {
            if (baseUri.Query != null && baseUri.Query.Length > 1)
                baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
            else
                baseUri.Query = queryToAppend;
            return baseUri;
        }
    }
}
