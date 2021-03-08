using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentitiesService.Models;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IApplicationRepository
    {
        dynamic PostApplication(ApplicationsModel model);
        dynamic PutApplication(ApplicationsModel model);
        dynamic GetApplication(int id, Pagination pageInfo);
        dynamic DeleteApplication(int id);
    }
}
