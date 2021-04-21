using System.Threading.Tasks;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IIdentitiesRepository
    {
        Task<Identities> PostIdentity(RegistrationDto registrationDto);
    }
}
