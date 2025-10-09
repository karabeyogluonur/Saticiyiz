using MediatR;
using ST.Application.Wrappers;

namespace ST.Application.Features.Identity.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<Response<string>>
    {
        public string Email { get; set; }
    }
}
