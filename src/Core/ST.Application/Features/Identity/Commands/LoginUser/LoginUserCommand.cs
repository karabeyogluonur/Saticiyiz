using MediatR;
using ST.Application.Wrappers;

namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<Response<LoginUserResultDto>>
    {
        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public bool RememberMe { get; set; } = true;
    }
}
