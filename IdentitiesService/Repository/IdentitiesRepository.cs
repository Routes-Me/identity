using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Obfuscation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly IUserIncludedRepository _userIncludedRepository;
        private readonly Dependencies _dependencies;
        public IdentitiesRepository(IOptions<AppSettings> appSettings, IdentitiesServiceContext context, IUserIncludedRepository userIncludedRepository, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _userIncludedRepository = userIncludedRepository;
            _dependencies = dependencies.Value;
        }

        public dynamic DeleteIdentity(string identityId)
        {
            try
            {
                var identityIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(identityId), _appSettings.PrimeInverse);
                var identities = _context.Identities.Include(x => x.PhoneIdentities).Include(x => x.EmailIdentity).Include(x => x.IdentitiesRoles).Where(x => x.IdentityId == identityIdDecrypted).FirstOrDefault();
                if (identities == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IdentityNotFound, StatusCodes.Status404NotFound);

                if (identities.IdentitiesRoles != null)
                {
                    _context.IdentitiesRoles.RemoveRange(identities.IdentitiesRoles);
                    _context.SaveChanges();
                }
                if (identities.PhoneIdentities != null)
                {
                    _context.PhoneIdentities.RemoveRange(identities.PhoneIdentities);
                    _context.SaveChanges();
                }
                if (identities.EmailIdentity != null)
                {
                    _context.EmailIdentities.Remove(identities.EmailIdentity);
                    _context.SaveChanges();
                }
                _context.Identities.Remove(identities);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.IdentityDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateIdentity(RegistrationModel model)
        {
            try
            {
                var identityIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.IdentityId), _appSettings.PrimeInverse);
                int institutionIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.InstitutionId), _appSettings.PrimeInverse);
                List<RolesModel> roles = new List<RolesModel>();
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var identity = _context.Identities.Include(x => x.IdentitiesRoles).Include(x => x.PhoneIdentities).Include(x => x.EmailIdentity)
                        .Where(x => x.IdentityId == identityIdDecrypted).FirstOrDefault();
                if (identity == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IdentityNotFound, StatusCodes.Status404NotFound);

                foreach (var role in model.Roles)
                {
                    var userRole = _context.Roles.Where(x => x.ApplicationId == ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse)
                    && x.PrivilegeId == ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (userRole == null)
                    {
                        return ReturnResponse.ErrorResponse(CommonMessage.UserRoleNotFound, StatusCodes.Status404NotFound);
                    }
                    else
                    {
                        RolesModel rolesModel = new RolesModel();
                        rolesModel.ApplicationId = ObfuscationClass.EncodeId(userRole.ApplicationId, _appSettings.Prime).ToString();
                        rolesModel.PrivilegeId = ObfuscationClass.EncodeId(userRole.PrivilegeId, _appSettings.Prime).ToString();
                        roles.Add(rolesModel);
                    }
                }

                if (identity.IdentitiesRoles != null)
                {
                    _context.IdentitiesRoles.RemoveRange(identity.IdentitiesRoles);
                    _context.SaveChanges();
                }

                foreach (var role in roles)
                {
                    IdentitiesRoles usersroles = new IdentitiesRoles()
                    {
                        IdentityId = identityIdDecrypted,
                        ApplicationId = ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse),
                        PrivilegeId = ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)
                    };
                    _context.IdentitiesRoles.Add(usersroles);
                }
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var userPhone = identity.PhoneIdentities.Where(x => x.IdentityId == identityIdDecrypted).FirstOrDefault();
                    if (userPhone == null)
                    {
                        PhoneIdentities newPhone = new PhoneIdentities()
                        {
                            CreatedAt = DateTime.Now,
                            PhoneNumber = model.PhoneNumber,
                            IdentityId = identityIdDecrypted
                        };
                        _context.PhoneIdentities.Add(newPhone);
                    }
                    else if (userPhone.PhoneNumber != model.PhoneNumber)
                    {
                        userPhone.PhoneNumber = model.PhoneNumber;
                        _context.PhoneIdentities.Update(userPhone);
                        _context.SaveChanges();
                    }
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    identity.EmailIdentity.Email = model.Email;
                }

                _context.Identities.Update(identity);
                _context.SaveChanges();

                if (institutionIdDecrypted != 0)
                {
                    string EncodedUserId = ObfuscationClass.EncodeId(identity.UserId, _appSettings.Prime).ToString();
                    var driverData = GetInstitutionIdsFromDrivers(EncodedUserId);

                    if (driverData == null || driverData.Count == 0)
                    {
                        DriversModel driver = new DriversModel()
                        {
                            InstitutionId = model.InstitutionId,
                            UserId = EncodedUserId
                        };

                        var client = new RestClient(_appSettings.Host + _dependencies.VehicleUrl);
                        var request = new RestRequest(Method.POST);
                        string jsonToSend = JsonConvert.SerializeObject(driver);
                        request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                        request.RequestFormat = DataFormat.Json;
                        IRestResponse institutionResponse = client.Execute(request);
                        if (institutionResponse.StatusCode != HttpStatusCode.Created) { }
                    }
                    else
                    {
                        string driverId = driverData.Where(x => x.UserId == EncodedUserId).Select(x => x.DriverId).FirstOrDefault();
                        DriversModel driver = new DriversModel()
                        {
                            DriverId = driverId,
                            InstitutionId = model.InstitutionId,
                            UserId = EncodedUserId
                        };
                        var client = new RestClient(_appSettings.Host + _dependencies.VehicleUrl);
                        var request = new RestRequest(Method.PUT);
                        string jsonToSend = JsonConvert.SerializeObject(driver);
                        request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                        request.RequestFormat = DataFormat.Json;
                        IRestResponse institutionResponse = client.Execute(request);
                        if (institutionResponse.StatusCode != HttpStatusCode.Created) { }
                    }
                }

                return ReturnResponse.SuccessResponse(CommonMessage.IdentityUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetIdentity(string identityId, Pagination pageInfo, string includeType)
        {
            try
            {
                var identityIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(identityId), _appSettings.PrimeInverse);
                int totalCount = 0;
                UsersGetResponse response = new UsersGetResponse();
                List<IdentitiesDto> IdentitiesDtoList = new List<IdentitiesDto>();
                if (identityIdDecrypted == 0)
                {
                    var identitiessData = _context.Identities.Include(x => x.PhoneIdentities).Include(x => x.EmailIdentity).AsEnumerable().ToList()
                            .GroupBy(p => p.IdentityId).Select(g => g.First()).ToList().OrderBy(a => a.IdentityId)
                            .Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
                    foreach (var item in identitiessData)
                    {
                        IdentitiesDto identitiesDto = new IdentitiesDto();
                        identitiesDto.IdentityId = ObfuscationClass.EncodeId(item.IdentityId, _appSettings.Prime).ToString();
                        identitiesDto.UserId = ObfuscationClass.EncodeId(item.UserId, _appSettings.Prime).ToString();
                        identitiesDto.Phone = item.PhoneIdentities.Where(x => x.IdentityId == item.IdentityId).Select(x => x.PhoneNumber).FirstOrDefault();
                        identitiesDto.Email = item.EmailIdentity.Email;
                        identitiesDto.CreatedAt = item.CreatedAt;
                        var identityRoles = (from identitiesRoles in _context.IdentitiesRoles
                                          where identitiesRoles.IdentityId == item.IdentityId
                                          select new RolesModel
                                          {
                                              ApplicationId = ObfuscationClass.EncodeId(identitiesRoles.ApplicationId, _appSettings.Prime).ToString(),
                                              PrivilegeId = ObfuscationClass.EncodeId(identitiesRoles.PrivilegeId, _appSettings.Prime).ToString()
                                          }).ToList();
                        identitiesDto.Roles = identityRoles;
                        IdentitiesDtoList.Add(identitiesDto);
                    }
                    totalCount = _context.Identities.ToList().Count();
                }
                else
                {
                    var usersData = _context.Identities.Include(x => x.PhoneIdentities).Where(x => x.IdentityId == identityIdDecrypted)
                        .AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    foreach (var item in usersData)
                    {
                        IdentitiesDto usersModel = new IdentitiesDto();
                        usersModel.UserId = ObfuscationClass.EncodeId(item.UserId, _appSettings.Prime).ToString();
                        usersModel.Phone = item.PhoneIdentities.Where(x => x.IdentityId == item.IdentityId).Select(x => x.PhoneNumber).FirstOrDefault();
                        usersModel.Email = item.EmailIdentity.Email;
                        usersModel.CreatedAt = item.CreatedAt;
                        var usersRoles = (from userroles in _context.IdentitiesRoles
                                          where userroles.IdentityId == item.UserId
                                          select new RolesModel
                                          {
                                              ApplicationId = ObfuscationClass.EncodeId(userroles.ApplicationId, _appSettings.Prime).ToString(),
                                              PrivilegeId = ObfuscationClass.EncodeId(userroles.PrivilegeId, _appSettings.Prime).ToString()
                                          }).ToList();
                        usersModel.Roles = usersRoles;
                        IdentitiesDtoList.Add(usersModel);
                    }
                    totalCount = _context.Identities.Where(x => x.UserId == identityIdDecrypted).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                dynamic includeData = new JObject();
                if (!string.IsNullOrEmpty(includeType))
                {
                    string[] includeArr = includeType.Split(',');
                    if (includeArr.Length > 0)
                    {
                        foreach (var item in includeArr)
                        {
                            if (item.ToLower() == "application" || item.ToLower() == "applications")
                                includeData.applications = _userIncludedRepository.GetApplicationIncludedData(IdentitiesDtoList);

                            else if (item.ToLower() == "privilege" || item.ToLower() == "privileges")
                                includeData.privileges = _userIncludedRepository.GetPrivilegeIncludedData(IdentitiesDtoList);
                        }
                    }
                }

                if (((JContainer)includeData).Count == 0)
                    includeData = null;

                response.message = CommonMessage.IdentityRetrived;
                response.statusCode = StatusCodes.Status200OK;
                response.status = true;
                response.pagination = page;
                response.data = IdentitiesDtoList;
                response.included = includeData;

                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public List<DriversGetModel> GetInstitutionIdsFromDrivers(string userId)
        {
            List<DriversGetModel> lstDrivers = new List<DriversGetModel>();
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.VehicleUrl);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = response.Content;
                        var driverData = JsonConvert.DeserializeObject<DriverGetResponse>(result);
                        lstDrivers.AddRange(driverData.data);
                    }
                }
                else
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.VehicleUrl + "?userId=" + userId);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = response.Content;
                        var driverData = JsonConvert.DeserializeObject<DriverGetResponse>(result);
                        lstDrivers.AddRange(driverData.data);
                    }
                }
                return lstDrivers;
            }
            catch (Exception)
            {
                return lstDrivers;
            }
        }
    }
}