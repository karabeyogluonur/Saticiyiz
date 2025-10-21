using AutoMapper;
using MediatR;
using ST.Application.DTOs.Billing;
using ST.Application.Interfaces.Billing;
using ST.Application.Wrappers;
using ST.Domain.Entities.Billing;


namespace ST.Application.Features.Billing.Queries.GetAllBillingProfile
{
    public class GetAllBillingProfilesQueryHandler : IRequestHandler<GetAllBillingProfilesQuery, Response<IEnumerable<BillingProfileDto>>>
    {
        private readonly IBillingProfileService _billingProfileService;
        private readonly IMapper _mapper;

        public GetAllBillingProfilesQueryHandler(IMapper mapper, IBillingProfileService billingProfileService)
        {
            _mapper = mapper;
            _billingProfileService = billingProfileService;
        }

        public async Task<Response<IEnumerable<BillingProfileDto>>> Handle(GetAllBillingProfilesQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<BillingProfile> billingProfiles = await _billingProfileService.GetAllBillingProfileAsync();

            IEnumerable<BillingProfileDto> mappedProfiles = _mapper.Map<IEnumerable<BillingProfileDto>>(billingProfiles);

            return Response<IEnumerable<BillingProfileDto>>.Success(mappedProfiles);
        }
    }
}