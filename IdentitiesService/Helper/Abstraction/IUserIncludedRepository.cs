using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Helper.Abstraction
{
    public interface IUserIncludedRepository
    {
        dynamic GetApplicationIncludedData(List<IdentitiesDto> usersModelList);
        dynamic GetPrivilegeIncludedData(List<IdentitiesDto> usersModelList);
    }
}
