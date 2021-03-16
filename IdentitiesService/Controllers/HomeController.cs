using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace IdentitiesService.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
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
            return "Identities service started successfully. Environment - " + _hostingEnv.EnvironmentName +"";
        }
    }
}
