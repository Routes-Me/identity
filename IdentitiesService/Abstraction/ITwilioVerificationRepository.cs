using System.Threading.Tasks;

namespace IdentitiesService.Abstraction
{
    public interface ITwilioVerificationRepository
    {
        Task<bool> TwilioVerificationResource(string phone);
        Task<bool> TwilioVerificationCheckResource(string phone, string code);
    }
}
