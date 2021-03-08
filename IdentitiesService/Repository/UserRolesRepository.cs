using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Obfuscation;
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
    public class UserRolesRepository : IRolesRepository
    {
        private readonly IdentitiesServiceContext _context;
        private readonly AppSettings _appSettings;

        public UserRolesRepository(IOptions<AppSettings> appSettings, IdentitiesServiceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic DeleteRoles(string ApplicationId, string PrivilegeId)
        {
            RolesResponse response = new RolesResponse();
            try
            {
                var ApplicationIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(ApplicationId), _appSettings.PrimeInverse);
                var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(PrivilegeId), _appSettings.PrimeInverse);
                var roles = _context.Roles.Where(x => x.ApplicationId == ApplicationIdDecrypted && x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (roles == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleNotFound, StatusCodes.Status404NotFound);

                var usersRole = _context.IdentitiesRoles.Where(x => x.ApplicationId == ApplicationIdDecrypted && x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (usersRole != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleConflict, StatusCodes.Status409Conflict);

                _context.IdentitiesRoles.RemoveRange(usersRole);
                _context.SaveChanges();
                _context.Roles.Remove(roles);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.RoleDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetRoles(string ApplicationId, string PrivilegeId, Pagination pageInfo)
        {
            RolesGetResponse response = new RolesGetResponse();
            int totalCount = 0;
            try
            {
                var ApplicationIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(ApplicationId), _appSettings.PrimeInverse);
                var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(PrivilegeId), _appSettings.PrimeInverse);
                List<RolesModel> userRolesModelList = new List<RolesModel>();
                if (ApplicationIdDecrypted == 0 && PrivilegeIdDecrypted == 0)
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          select new RolesModel()
                                          {
                                              ApplicationId = ObfuscationClass.EncodeId(userRole.ApplicationId, _appSettings.Prime).ToString(),
                                              PrivilegeId = ObfuscationClass.EncodeId(userRole.PrivilegeId, _appSettings.Prime).ToString(),
                                          }).AsEnumerable().OrderBy(a => a.ApplicationId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.ToList().Count();
                }
                else
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          where userRole.PrivilegeId == PrivilegeIdDecrypted && userRole.ApplicationId == ApplicationIdDecrypted
                                          select new RolesModel()
                                          {
                                              ApplicationId = ObfuscationClass.EncodeId(userRole.ApplicationId, _appSettings.Prime).ToString(),
                                              PrivilegeId = ObfuscationClass.EncodeId(userRole.PrivilegeId, _appSettings.Prime).ToString(),
                                          }).AsEnumerable().OrderBy(a => a.ApplicationId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.Where(x => x.PrivilegeId == PrivilegeIdDecrypted && x.ApplicationId == ApplicationIdDecrypted).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.RoleRetrived;
                response.pagination = page;
                response.data = userRolesModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic InsertRoles(RolesModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var ApplicationIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.ApplicationId), _appSettings.PrimeInverse);
                var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.PrivilegeId), _appSettings.PrimeInverse);

                Roles role = new Roles()
                {
                    ApplicationId = ApplicationIdDecrypted,
                    PrivilegeId = PrivilegeIdDecrypted
                };
                _context.Roles.Add(role);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.RoleInsert, true);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateRoles(RolesModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var ApplicationIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.ApplicationId), _appSettings.PrimeInverse);
                var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.PrivilegeId), _appSettings.PrimeInverse);

                var roles = _context.Roles.Where(x => x.ApplicationId == ApplicationIdDecrypted && x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (roles == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleNotFound, StatusCodes.Status404NotFound);

                roles.ApplicationId = ApplicationIdDecrypted;
                roles.PrivilegeId = PrivilegeIdDecrypted;
                _context.Roles.Update(roles);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.RoleUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}