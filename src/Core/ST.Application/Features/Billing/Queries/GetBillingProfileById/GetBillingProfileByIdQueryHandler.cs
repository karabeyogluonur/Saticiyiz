using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ST.Application.DTOs.Billing;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Billing;
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers;
using ST.Domain.Entities.Billing;

namespace ST.Application.Features.Billing.Queries.GetBillingProfileById
{
    public class GetBillingProfileByIdQueryHandler : IRequestHandler<GetBillingProfileByIdQuery, Response<BillingProfileDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBillingProfileService _billingProfileService;

        public GetBillingProfileByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IBillingProfileService billingProfileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _billingProfileService = billingProfileService;
        }

        public async Task<Response<BillingProfileDto>> Handle(GetBillingProfileByIdQuery request, CancellationToken cancellationToken)
        {
            BillingProfile billingProfile = await _billingProfileService.GetBillingProfileByIdAsync(request.Id);

            if (billingProfile == null)
                throw new NotFoundException("Fatura profili bulunamadı.");

            var mappedBillingProfile = _mapper.Map<BillingProfileDto>(billingProfile);
            return Response<BillingProfileDto>.Success(mappedBillingProfile);
        }
    }
}