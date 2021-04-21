using Microsoft.AspNetCore.Mvc;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
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
            try
            {
                Identities identity = await _identitesRepository.PostIdentity(registrationDto);
                _context.Identities.Add(identity);
                await _context.SaveChangesAsync();
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
            return StatusCode(StatusCodes.Status201Created, CommonMessage.IdentityInsert);
        }
    }
}