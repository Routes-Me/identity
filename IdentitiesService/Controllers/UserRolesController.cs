using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Controllers
{
    [Route("api")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolesRepository _rolesRepository;
        public RolesController(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }

        [HttpGet]
        [Route("roles")]
        public IActionResult Get(string ApplicationId, string PrivilegeId, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _rolesRepository.GetRoles(ApplicationId, PrivilegeId, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("roles")]
        public IActionResult Post(RolesModel model)
        {
            dynamic response = _rolesRepository.InsertRoles(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("roles")]
        public IActionResult Put(RolesModel model)
        {
            dynamic response = _rolesRepository.UpdateRoles(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("roles")]
        public IActionResult Delete(string ApplicationId, string PrivilegeId)
        {
            dynamic response = _rolesRepository.DeleteRoles( ApplicationId,  PrivilegeId);
            return StatusCode((int)response.statusCode, response);
        }
    }
}
