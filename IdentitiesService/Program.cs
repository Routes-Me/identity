using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentitiesService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string standardVersion = "Standard version: " + "{0}.{1}.{2}";
            Version standard = new Version(1, 0, 0);
            Console.WriteLine(standardVersion, standard.Major, standard.Minor, standard.Build);

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("twilio.json", optional: false, reloadOnChange: false);
            })
                .UseStartup<Startup>();
        }
    }
}
