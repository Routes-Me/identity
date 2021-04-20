using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IApplicationRepository
    {
        dynamic PostApplication(ApplicationsDto model);
        dynamic PutApplication(ApplicationsDto model);
        dynamic GetApplication(int id, Pagination pageInfo);
        dynamic DeleteApplication(int id);
    }
}
