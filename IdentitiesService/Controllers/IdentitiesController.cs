using Microsoft.AspNetCore.Mvc;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using RoutesSecurity;
using IdentitiesService.Models.DBModels;

namespace IdentitiesService.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class IdentitiesController : ControllerBase
    {
        private readonly IIdentitiesRepository _identitesRepository;
        private readonly IdentitiesServiceContext _context;
        public IdentitiesController(IIdentitiesRepository identitiesRepository, IdentitiesServiceContext context)
        {
            _identitesRepository = identitiesRepository;
            _context = context;
        }

        [HttpPost]
        [Route("identities")]
        public async Task<IActionResult> PostIdentity(RegistrationDto registrationDto)
        {
            PostIdentityResponse response = new PostIdentityResponse();
            try
            {
                Identities identity = await _identitesRepository.PostIdentity(registrationDto);
                _context.Identities.Add(identity);
                await _context.SaveChangesAsync();
                response.IdentityId = Obfuscation.Encode(identity.IdentityId);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            response.Message = CommonMessage.IdentityInsert;
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpDelete]
        [Route("identities/{identityId}")]
        public async Task<IActionResult> DeleteIdentity(string identityId)
        {
            try
            {
                Identities identity = _identitesRepository.DeleteIdentity(identityId);
                _context.Identities.Remove(identity);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}