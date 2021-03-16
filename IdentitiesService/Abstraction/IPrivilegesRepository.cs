using System.Threading.Tasks;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IPrivilegesRepository
    {
        dynamic PostPrivilege(PrivilegesModel model);
        dynamic PutPrivilege(PrivilegesModel model);
        dynamic GetPrivilege(int id, Pagination pageInfo);
        dynamic DeletePrivilege(int id);
    }
}
