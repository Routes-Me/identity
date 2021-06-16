using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Models
{
    public class Response
    {
        public bool status { get; set; }
        public string message { get; set; }
        public int statusCode { get; set; }
    }

    public class ErrorResponse
    {
        public string error { get; set; }
    }

    public class ReturnResponse
    {
        public static dynamic ExceptionResponse(Exception ex)
        {
            Response response = new Response();
            response.status = false;
            response.message = CommonMessage.ExceptionMessage + ex.Message;
            response.statusCode = StatusCodes.Status500InternalServerError;
            return response;
        }

        public static dynamic SuccessResponse(string message, bool isCreated)
        {
            Response response = new Response();
            response.status = true;
            response.message = message;
            if (isCreated)
                response.statusCode = StatusCodes.Status201Created;
            else
                response.statusCode = StatusCodes.Status200OK;
            return response;
        }

        public static dynamic ErrorResponse(string message, int statusCode)
        {
            Response response = new Response();
            response.status = false;
            response.message = message;
            response.statusCode = statusCode;
            return response;
        }
    }

    #region Roles Response
    public class RolesResponse : Response { }
    public class RolesGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<RolesModel> data { get; set; }
    }
    #endregion

    #region Login Response

    public class SignInResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class AuthenticationResponse
    {
        public Identities identity { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }

    public class TokenRenewalResponse
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string message { get; set; }
    }

    public class PostIdentityResponse
    {
        public string Message { get; set; }
        public string IdentityId { get; set; }
    }

    public class InvitationTokenResponse
    {
        public string invitationToken { get; set; }
    }
    #endregion

    public class PrivilegesResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<PrivilegesDto> data { get; set; }
    }

    public class ApplicationResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<ApplicationsDto> data { get; set; }
    }

    public class IdentifierResponse
    {
        public long Identifier { get; set; }
    }

    public class OfficersGetResponse
    {
        public Pagination pagination { get; set; }
        public List<OfficersDto> data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }
}
