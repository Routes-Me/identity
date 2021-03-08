using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Controllers
{
    [Route("api")]
    [ApiController]
    public class IdentitiesController : ControllerBase
    {
        private readonly IIdentitiesRepository _identitesRepository;
        private static readonly HttpClient HttpClient = new HttpClient();
        public IdentitiesController(IIdentitiesRepository identitiesRepository)
        {
            _identitesRepository = identitiesRepository;
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
        public IActionResult Put(RegistrationModel model)
        {
            dynamic response = _identitesRepository.UpdateIdentity(model);
            return StatusCode((int)response.statusCode, response);
        }
    }
}