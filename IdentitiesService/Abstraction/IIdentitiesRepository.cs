using IdentitiesService.Models.ResponseModel;

namespace IdentitiesService.Abstraction
{
    public interface IIdentitiesRepository
    {
        dynamic RevokeRefreshToken(string refreshToken);
        dynamic RevokeAccessToken(string accessToken);
    }
}
