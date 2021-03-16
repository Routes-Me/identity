using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using IdentitiesService.Abstraction;
using IdentitiesService.Helper.Abstraction;
using IdentitiesService.Helper.Repository;
using IdentitiesService.Models.Common;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Repository;
using IdentitiesService.Helper.CronJobServices;
using IdentitiesService.Helper.CronJobServices.CronJobExtensionMethods;

namespace IdentitiesService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddCronJob<ExpiredTokens>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"55 23 * * *"; // Runs every day at 23:55:00
            });

            services.AddDbContext<IdentitiesServiceContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.Configure<SendGridSettings>(Configuration.GetSection("SendGridSettings"));
            services.AddScoped<IRolesRepository, UserRolesRepository>();
            services.AddScoped<IIdentitiesRepository, IdentitiesRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IHelperRepository, HelperRepository>();
            services.AddScoped<IPasswordHasherRepository, PasswordHasherRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IPrivilegesRepository, PrivilegesRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<IUserIncludedRepository, UserIncludedRepository>();
            services.AddScoped<IIdentitiesRepository, IdentitiesRepository>();
            services.AddSingleton<ITwilioVerificationRepository>(new TwilioVerificationRepository(
                Configuration.GetSection("Twilio").Get<Configuration.Twilio>()));
            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            var dependenciessSection = Configuration.GetSection("Dependencies");
            services.Configure<Dependencies>(dependenciessSection);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer("clientSecretKey", x =>
             {
                 x.RequireHttpsMetadata = false;
                 x.SaveToken = true;
                 x.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = appSettings.ValidAudience,
                     ValidIssuer = appSettings.ValidIssuer,
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Secret)),
                    // verify signature to avoid tampering
                    ValidateLifetime = true, // validate the expiration
                    RequireExpirationTime = true,
                     ClockSkew = TimeSpan.FromMinutes(5) // tolerance for the expiration date
                };
                 x.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                         logger.LogError("Authentication failed.", context.Exception);
                         context.Response.StatusCode = 401;
                         if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                         {
                             context.Response.Headers.Add("Token-Expired", "true");
                         }
                         context.Response.OnStarting(async () =>
                         {
                             context.Response.ContentType = "application/json";
                             ErrorMessage errorMessage = new ErrorMessage();
                             errorMessage.Message = "Authentication failed.";
                             await context.Response.WriteAsync(JsonConvert.SerializeObject(errorMessage));
                         });
                         return Task.CompletedTask;
                     },
                     OnMessageReceived = context =>
                     {
                         return Task.CompletedTask;
                     },
                     OnChallenge = context =>
                     {
                         var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                         logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
                         return Task.CompletedTask;
                     }
                 };
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("MyPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
