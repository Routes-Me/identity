using System;
using System.Threading.Tasks;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;
using IdentitiesService.Models.DBModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace IdentitiesService.Controllers
{
    [Route("api")]
    [ApiController]
    public class IdentitiesController : ControllerBase
    {
        private readonly IIdentitiesRepository _identitiesRepository;
        private readonly IdentitiesServiceContext _context;
        public IdentitiesController(IIdentitiesRepository identitiesRepository, IdentitiesServiceContext context)
        {
            _identitiesRepository = identitiesRepository;
            _context = context;
        }

        [HttpPost]
        [Route("identities/tokens/revoke-access")]
        public async Task<IActionResult> RevokeAccessToken(RevokedAccessTokenDto revokedAccessTokenDto)
        {
            try
            {
                RevokedAccessTokens revokedToken = _identitiesRepository.RevokeAccessToken(revokedAccessTokenDto.AccessToken);
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
                RevokedRefreshTokens revokedToken = _identitiesRepository.RevokeRefreshToken(revokedRefreshTokenDto.RefreshToken);
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
    }
}
