namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginResultDto
    {
        public LoginStatus Status { get; set; }
        public bool RequiresSetup { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum LoginStatus
    {
        Success,
        InvalidCredentials,
        RequiresTwoFactor,
        LockedOut,
        NotAllowed
    }
}