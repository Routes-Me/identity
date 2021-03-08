using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Verify.V2.Service;
using IdentitiesService.Abstraction;
using IdentitiesService.Models.Common;

namespace IdentitiesService.Repository
{
    public class TwilioVerificationRepository : ITwilioVerificationRepository
    {
        private readonly Configuration.Twilio _config;

        public TwilioVerificationRepository(Configuration.Twilio configuration)
        {
            _config = configuration;
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
        }
        public async Task<bool> TwilioVerificationResource(string phone)
        {
            try
            {
                var verificationResource = await VerificationResource.CreateAsync(
                    //to: "+965" + phone,
                    to: "+91" + phone,
                    channel: "sms",
                    pathServiceSid: _config.VerificationSid
                );
                var result = new VerificationResult(verificationResource.Sid);
                if (result.IsValid)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> TwilioVerificationCheckResource(string phone, string code)
        {
            try
            {
                var verificationCheckResource = await VerificationCheckResource.CreateAsync(
                    //to: "+965" + phone,
                    to: "+91" + phone,
                    code: code,
                    pathServiceSid: _config.VerificationSid
                );
                var result = verificationCheckResource.Status.Equals("approved") ?
                    new VerificationResult(verificationCheckResource.Sid) :
                    new VerificationResult(new List<string> { "Wrong code. Try again." });

                if (result.IsValid)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
