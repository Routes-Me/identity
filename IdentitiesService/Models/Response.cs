using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using IdentitiesService.Models.DBModels;
using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Models
{

    #region Login Response

    public class SignInResponse 
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string token { get; set; }
    }

    public class AuthenticationResponse
    {
        public Identities user { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }

    public class TokenRenewalResponse
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string message { get; set; }
    }
    #endregion

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
