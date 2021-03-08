using System.Threading.Tasks;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IIdentitiesRepository
    {
        dynamic DeleteIdentity(string identityId);
        dynamic UpdateIdentity(RegistrationModel model);
        dynamic GetIdentity(string identityId, Pagination pageInfo, string includeType);
    }
}
