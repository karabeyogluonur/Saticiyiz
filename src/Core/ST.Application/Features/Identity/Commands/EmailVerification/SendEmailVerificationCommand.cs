using MediatR;
using ST.Application.Wrappers;

namespace ST.Application.Features.Identity.Commands.EmailVerification
{
    public class SendEmailVerificationCommand : IRequest<Response<string>>
    {
        public SendEmailVerificationCommand(string email)
        {
            Email = email;
        }
        public string Email { get; set; }
    }
}
