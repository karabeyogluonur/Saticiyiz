using MediatR;
using ST.Application.Wrappers;

namespace ST.Application.Features.Identity.Commands.EmailVerification
{
    public class VerifyEmailCommand : IRequest<Response<string>>
    {
        public string Token { get; set; }

        public VerifyEmailCommand(string token)
        {
            Token = token;
        }
    }
}
