using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentitiesService.Abstraction;
using IdentitiesService.Models;
using IdentitiesService.Models.Common;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Repository
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IdentitiesServiceContext _context;
        private readonly AppSettings _appSettings;

        public ApplicationRepository(IOptions<AppSettings> appSettings, IdentitiesServiceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic DeleteApplication(int id)
        {
            try
            {
                var ApplicationIdDecrypted = Obfuscation.Decode(id.ToString());
                var applicationData = _context.Applications.Include(x => x.Roles).Where(x => x.ApplicationId == ApplicationIdDecrypted).FirstOrDefault();
                if (applicationData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.ApplicationNotFound, StatusCodes.Status404NotFound);

                if (applicationData.Roles.Count > 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.ApplicationAssociatedWithRole, StatusCodes.Status409Conflict);

                var userRoleData = _context.IdentitiesRoles.Where(x => x.ApplicationId == ApplicationIdDecrypted).FirstOrDefault();
                if (userRoleData != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.ApplicationAssociatedWithUserRole, StatusCodes.Status409Conflict);

                _context.Applications.Remove(applicationData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.ApplicationDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetApplication(int id, Pagination pageInfo)
        {
            ApplicationResponse response = new ApplicationResponse();
            int totalCount = 0;
            try
            {
                List<ApplicationsModel> ApplicationsModelList = new List<ApplicationsModel>();
                if (id == 0)
                {
                    ApplicationsModelList = (from application in _context.Applications
                                             select new ApplicationsModel()
                                             {
                                                 ApplicationId = Obfuscation.Encode(application.ApplicationId),
                                                 Name = application.Name,
                                                 CreatedAt =application.CreatedAt
                                             }).AsEnumerable().OrderBy(a => a.ApplicationId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.ToList().Count();
                }
                else
                {
                    var ApplicationIdDecrypted = Obfuscation.Decode(id.ToString());
                    ApplicationsModelList = (from application in _context.Applications
                                             where application.ApplicationId == ApplicationIdDecrypted
                                             select new ApplicationsModel()
                                             {
                                                 ApplicationId = Obfuscation.Encode(application.ApplicationId),
                                                 Name = application.Name,
                                                 CreatedAt = application.CreatedAt
                                             }).AsEnumerable().OrderBy(a => a.ApplicationId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.Where(x => x.ApplicationId == ApplicationIdDecrypted).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.ApplicationRetrived;
                response.pagination = page;
                response.data = ApplicationsModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic PostApplication(ApplicationsModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PassValidData, StatusCodes.Status400BadRequest);

                var applicationData = _context.Applications.Where(x => x.Name.ToLower() == model.Name.ToLower()).FirstOrDefault();
                if (applicationData != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.ApplicationExists, StatusCodes.Status409Conflict);

                Applications applications = new Applications()
                {
                    Name = model.Name,
                    CreatedAt = DateTime.Now
                };
                _context.Applications.Add(applications);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.ApplicationInsert, true);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic PutApplication(ApplicationsModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PassValidData, StatusCodes.Status400BadRequest);

                var ApplicationIdDecrypted = Obfuscation.Decode(model.ApplicationId);

                var applicationData = _context.Applications.Where(x => x.ApplicationId == ApplicationIdDecrypted).FirstOrDefault();
                if (applicationData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.ApplicationNotFound, StatusCodes.Status404NotFound);

                var IsApplication = _context.Applications.Where(x => x.Name.ToLower() == model.Name.ToLower() && x.ApplicationId != ApplicationIdDecrypted).FirstOrDefault();
                if (IsApplication != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.ApplicationExists, StatusCodes.Status409Conflict);

                applicationData.Name = model.Name;
                _context.Applications.Update(applicationData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.ApplicationUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}
