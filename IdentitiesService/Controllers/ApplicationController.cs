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
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationRepository _applicationRepository;
        public ApplicationController(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        [HttpPost]
        [Route("applications")]
        public IActionResult Post(ApplicationsModel model)
        {
            dynamic response = _applicationRepository.PostApplication(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("applications")]
        public IActionResult Put(ApplicationsModel model)
        {
            dynamic response = _applicationRepository.PutApplication(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpGet]
        [Route("applications/{id=0}")]
        public IActionResult Get(int id, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _applicationRepository.GetApplication(id, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("applications/{id}")]
        public IActionResult Delete(int id)
        {
            dynamic response = _applicationRepository.DeleteApplication(id);
            return StatusCode((int)response.statusCode, response);
        }
    }
}
