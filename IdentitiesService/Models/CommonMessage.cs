namespace IdentitiesService.Models
{
    public class CommonMessage
    {
        public static string PassValidData = "Some data are missed in the request. Pass valid data.";
        public static string PhoneExist = "Phone number already exist.";   
        public static string EmailExist = "Email already exist.";
        public static string IdentityInsert = "Identity created successfully.";
        public static string MissingUserName = "Missing username.";
        public static string MissingPassword = "Missing password.";
        public static string IncorrectUsername = "Incorrect username.";
        public static string IncorrectPassword = "Incorrect password.";
        public static string IncorrectUserRole = "Incorrect user role.";
        public static string LoginSuccess = "Login successfully.";
        public static string RenewSuccess = "Tokens Renewed successfully.";
        public static string ExceptionMessage = "Something went wrong. Error Message - ";
        public static string PrivilegeNotFound = "Privileges not found.";
        public static string ApplicationNotFound = "Applications not found.";
        public static string UnknownApplication = "No Application is specified in request";
        public static string TokenDataNull = "Token data is null.";
        public static string Unauthorized = "You are unauthorized to perform this operation.";
        public static string Forbidden = "Forbidden";
        public static string InvalidData = "Pass valid data.";
        public static string TokenExpired = "Token is expired.";
        public static string TokenAlreadyRevoked = "This token is already revoked.";
        public static string InvalidIdentityToken = "This token does not belong to a valid identity.";
        public static string OfficerNotFound = "Officer not found.";
        public static string RefreshTokenRevoked = "Refresh token revoked successfully.";
        public static string AccessTokenRevoked = "Access token revoked successfully.";
        public static string ConnectionFailure = "Request cannot be executed: unable to establish connection with the targeted machine.";
    }
}       
