using System.Net.Http;
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
    [Route("api")]
    [ApiController]
    public class IdentitiesController : ControllerBase
    {
        private readonly IIdentitiesRepository _identitesRepository;
        private readonly IAccountRepository _accountsRepository;
        private readonly IdentitiesServiceContext _context;
        public IdentitiesController(IIdentitiesRepository identitiesRepository, IAccountRepository accountsRepository, IdentitiesServiceContext context)
        {
            _identitesRepository = identitiesRepository;
            _accountsRepository = accountsRepository;
            _context = context;
        }

        [HttpGet]
        [Route("identities/{identityId?}")]
        public IActionResult Get(string identityId, string Include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _identitesRepository.GetIdentity(identityId, pageInfo, Include);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("identities/{identityId}")]
        public IActionResult delete(string identityId)
        {
            dynamic response = _identitesRepository.DeleteIdentity(identityId);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("identities")]
        public IActionResult Put(UpdateIdentitiesDto updateIdentitiesDto)
        {
            dynamic response = _identitesRepository.UpdateIdentity(updateIdentitiesDto);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("identities")]
        public async Task<IActionResult> Signup(RegistrationDto registrationDto)
        {
            try
            {
                Identities identity = await _accountsRepository.SignUp(registrationDto);
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