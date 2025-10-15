using MediatR;
using ST.Application.Common.Attributes;
using ST.Application.Wrappers;

namespace ST.Application.Features.Identity.Commands.RegisterUser
{
    [Transactional]
    public class RegisterUserCommand : IRequest<Response<int>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}
