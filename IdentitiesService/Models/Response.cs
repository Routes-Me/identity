using Microsoft.AspNetCore.Http;
using System;

namespace IdentitiesService.Models
{
    public class Response
    {
        public string message { get; set; }
        public int statusCode { get; set; }
    }
}
