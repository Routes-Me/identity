using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentitiesService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [Obsolete]
        public readonly IHostingEnvironment _hostingEnv;

        [Obsolete]
        public HomeController(IHostingEnvironment hostingEnv)
        {
            _hostingEnv = hostingEnv;
        }
        [HttpGet]
        [Obsolete]
        public string Get()
        {
            return "Identities service started successfully. Environment - " + _hostingEnv.EnvironmentName + "";
        }
    }
}
