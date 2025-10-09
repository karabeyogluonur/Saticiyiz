using MediatR;
using ST.Application.Wrappers;

namespace ST.Application.Features.Identity.Commands.UnsubscribeNewsletter
{
    public class UnsubscribeNewsletterCommand : IRequest<Response<string>>
    {
        public string Token { get; set; }

        public UnsubscribeNewsletterCommand(string token)
        {
            Token = token;
        }
    }
}
