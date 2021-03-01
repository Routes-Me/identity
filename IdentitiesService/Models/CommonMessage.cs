namespace IdentitiesService.Models
{
    public class CommonMessage
    {
        public static string ExceptionMessage = "Something went wrong. Error Message - ";
        public static string InvalidData = "Pass valid data.";
        public static string TokenExpired = "Token is expired.";
        public static string TokenAlreadyRevoked = "This token is already revoked.";
        public static string RefreshTokenRevoked = "Refresh token revoked successfully.";
        public static string AccessTokenRevoked = "Access token revoked successfully.";
    }
}
