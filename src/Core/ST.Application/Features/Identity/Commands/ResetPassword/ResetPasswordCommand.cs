using MediatR;
using ST.Application.Wrappers;

namespace ST.Application.Features.Identity.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<Response<string>>
    {
        public string Token { get; set; } = String.Empty;
        public string NewPassword { get; set; } = String.Empty;
    }

}