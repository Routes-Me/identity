using Microsoft.AspNetCore.Mvc;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class RolesVersionedController : ControllerBase
    {
        private readonly IRolesRepository _rolesRepository;
        public RolesVersionedController(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }

        [HttpGet]
        [Route("roles")]
        public IActionResult Get(string ApplicationId, string PrivilegeId, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _rolesRepository.GetRoles(ApplicationId, PrivilegeId, pageInfo);
            return StatusCode(response.statusCode, response);
        }

        [HttpPost]
        [Route("roles")]
        public IActionResult Post(RolesModel model)
        {
            dynamic response = _rolesRepository.InsertRoles(model);
            return StatusCode(response.statusCode, response);
        }

        [HttpPut]
        [Route("roles")]
        public IActionResult Put(RolesModel model)
        {
            dynamic response = _rolesRepository.UpdateRoles(model);
            return StatusCode(response.statusCode, response);
        }

        [HttpDelete]
        [Route("roles")]
        public IActionResult Delete(string ApplicationId, string PrivilegeId)
        {
            dynamic response = _rolesRepository.DeleteRoles( ApplicationId,  PrivilegeId);
            return StatusCode(response.statusCode, response);
        }
    }
}
