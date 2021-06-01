using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentitiesService.Abstraction;
using IdentitiesService.Helper.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.Common;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Repository
{
    public class IdentitiesRepository : IIdentitiesRepository
    {
        private readonly IdentitiesServiceContext _context;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasherRepository _passwordHasherRepository;

        public IdentitiesRepository(IOptions<AppSettings> appSettings, IdentitiesServiceContext context, IOptions<Dependencies> dependencies, IAccountRepository accountRepository, IPasswordHasherRepository passwordHasherRepository)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _dependencies = dependencies.Value;
            _accountRepository = accountRepository;
            _passwordHasherRepository = passwordHasherRepository;
        }

        public async Task<Identities> PostIdentity(RegistrationDto registrationDto)
        {
            if (registrationDto == null || registrationDto.Roles == null || string.IsNullOrEmpty(registrationDto.PhoneNumber) || string.IsNullOrEmpty(registrationDto.Email) || string.IsNullOrEmpty(registrationDto.Roles.Application) || string.IsNullOrEmpty(registrationDto.Roles.Privilege))
                throw new ArgumentNullException(CommonMessage.PassValidData);

            var email = _context.EmailIdentities.Where(x => x.Email == registrationDto.Email).FirstOrDefault();
            if (email != null)
                throw new ArgumentException(CommonMessage.EmailExist);

            string originalPassword = string.Empty;
            if (!string.IsNullOrEmpty(registrationDto.Password))
            {
                originalPassword = await _accountRepository.PasswordDecryptionAsync(registrationDto.Password);
                if (originalPassword == "Unauthorized Access")
                    throw new Exception(CommonMessage.IncorrectPassword);
            }

            Identities identity = new Identities()
            {
                UserId = Obfuscation.Decode(registrationDto.UserId),
                CreatedAt = DateTime.Now,
                EmailIdentity = new EmailIdentities
                {
                    Email = registrationDto.Email,
                    CreatedAt = DateTime.Now,
                    Password = _passwordHasherRepository.Hash(originalPassword)
                },
                PhoneIdentities = new List<PhoneIdentities>
                {
                    new PhoneIdentities
                    {
                        PhoneNumber = registrationDto.PhoneNumber,
                        CreatedAt = DateTime.Now
                    }
                },
                IdentitiesRoles = new List<IdentitiesRoles>()
                {
                    new IdentitiesRoles
                    {
                        ApplicationId = registrationDto.Roles.Application.Any(char.IsDigit) ? Obfuscation.Decode(registrationDto.Roles.Application) : GetApplicationId(registrationDto.Roles.Application),
                        PrivilegeId = registrationDto.Roles.Privilege.Any(char.IsDigit) ? Obfuscation.Decode(registrationDto.Roles.Privilege) : GetPrivilegeId(registrationDto.Roles.Privilege)
                    }
                }
            };
            return identity;
        }

        public Identities DeleteIdentity(string identityId)
        {
            if (string.IsNullOrEmpty(identityId))
                throw new ArgumentNullException(CommonMessage.PassValidData);

            Identities identity = _context.Identities.Include(i => i.EmailIdentity).Include(i => i.IdentitiesRoles).Include(i => i.PhoneIdentities).Where(i => i.IdentityId == Obfuscation.Decode(identityId)).FirstOrDefault();
            if (identity == null)
                throw new KeyNotFoundException(CommonMessage.IdentityNotFound);

            return identity;
        }

        private int GetApplicationId(string applicationName)
        {
            Applications application = _context.Applications.Where(a => a.Name == applicationName).FirstOrDefault();
            if (application == null)
                throw new ArgumentNullException(CommonMessage.ApplicationNotFound);
            return application.ApplicationId;
        }

        private int GetPrivilegeId(string privilegeName)
        {
            Privileges privilege = _context.Privileges.Where(a => a.Name == privilegeName).FirstOrDefault();
            if (privilege == null)
                throw new ArgumentNullException(CommonMessage.PrivilegeNotFound);
            return privilege.PrivilegeId;
        }
    }
}