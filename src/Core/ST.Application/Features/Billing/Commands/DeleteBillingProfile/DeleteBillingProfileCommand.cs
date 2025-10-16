using MediatR;
using ST.Application.Wrappers;

namespace ST.Application.Features.Billing.Commands.DeleteBillingProfile
{
    public class DeleteBillingProfileCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }
}