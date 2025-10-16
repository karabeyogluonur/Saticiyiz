using MediatR;
using ST.Application.DTOs.Billing;
using ST.Application.Wrappers;

namespace ST.Application.Features.Billing.Queries.GetAllBillingProfile
{
    public class GetAllBillingProfilesQuery : IRequest<Response<IEnumerable<BillingProfileDto>>>
    {
    }
}