namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginUserResponseDto
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
