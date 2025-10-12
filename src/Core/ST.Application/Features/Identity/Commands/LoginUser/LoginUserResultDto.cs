namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginUserResultDto
    {
        public LoginStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
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
