using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;
using IdentitiesService.Models.DBModels;

namespace IdentitiesService.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IdentitiesServiceContext _context;
        public AccountController(IAccountRepository accountRepository, IdentitiesServiceContext context)
        {
            _accountRepository = accountRepository;
            _context = context;
        }

        [HttpPost]
        [Route("authentications")]
        public async Task<IActionResult> AuthenticateUser(SigninDto signinDto)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            try
            {
                StringValues application;
                Request.Headers.TryGetValue("Application", out application);
                authenticationResponse = await _accountRepository.AuthenticateUser(signinDto, application.FirstOrDefault());
                _context.Identities.Update(authenticationResponse.identity);
                _context.SaveChanges();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (HttpListenerException ex)
            {
                return StatusCode(ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            SignInResponse response = new SignInResponse
            {
                Token = authenticationResponse.accessToken,
                RefreshToken = authenticationResponse.refreshToken
            };

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            };
            Response.Cookies.Append("refreshToken", authenticationResponse.refreshToken, cookieOptions);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPost]
        [Route("authentications/renewals")]
        public IActionResult RenewTokens(RefreshTokenDto tokenRenewModel)
        {
            TokenRenewalResponse response = new TokenRenewalResponse();
            try
            {
                StringValues accessToken;
                Request.Headers.TryGetValue("Authorization", out accessToken);
                accessToken = accessToken.ToString().Split(' ').LastOrDefault();
                response = _accountRepository.RenewTokens(tokenRenewModel.RefreshToken, accessToken);
            }
            catch (SecurityTokenExpiredException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (AccessViolationException)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            response.message = CommonMessage.RenewSuccess;
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPost]
        [Route("identities/tokens/revoke-access")]
        public async Task<IActionResult> RevokeAccessToken(RevokedAccessTokenDto revokedAccessTokenDto)
        {
            try
            {
                RevokedAccessTokens revokedToken = _accountRepository.RevokeAccessToken(revokedAccessTokenDto.AccessToken);
                await _context.RevokedAccessTokens.AddAsync(revokedToken);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (SecurityTokenExpiredException ex)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            return StatusCode(StatusCodes.Status200OK, CommonMessage.AccessTokenRevoked);
        }

        [HttpPost]
        [Route("identities/tokens/revoke-refresh")]
        public async Task<IActionResult> RevokeRefreshToken(RevokedRefreshTokenDto revokedRefreshTokenDto)
        {
            try
            {
                RevokedRefreshTokens revokedToken = _accountRepository.RevokeRefreshToken(revokedRefreshTokenDto.RefreshToken);
                await _context.RevokedRefreshTokens.AddAsync(revokedToken);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (SecurityTokenExpiredException ex)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            return StatusCode(StatusCodes.Status200OK, CommonMessage.RefreshTokenRevoked);
        }

        [HttpGet]
        [Route("tokens/invitations")]
        public IActionResult GenerateInvitationToken()
        {
            InvitationTokenResponse response = new InvitationTokenResponse();
            try
            {
                response.invitationToken = _accountRepository.GenerateInvitationToken();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponse{ error = ex.Message });
            }
            return StatusCode(StatusCodes.Status200OK, response);
        }
        [HttpPost]
        [Route("authentications/number")]
        public IActionResult AuthenticatePhoneNumber(string number, string verificationToken)
        {
            if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(verificationToken))
            {
                try
                {
                    StringValues application;
                    Request.Headers.TryGetValue("application", out application);
                    var authentication = _accountRepository.AuthenticatePhoneNumber(number, verificationToken, application);
                    if (authentication == null)
                        throw new ArgumentNullException(CommonMessage.InvalidData);
                    var response = new SignInResponse()
                    {
                        Token = authentication.Token,
                        RefreshToken = authentication.RefreshToken
                    };
                    return StatusCode(StatusCodes.Status200OK, response);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, ReturnResponse.ExceptionResponse(ex));
                }
            }
            else
            {
                throw new ArgumentNullException(CommonMessage.InvalidData);
            }
        }
    }
}
