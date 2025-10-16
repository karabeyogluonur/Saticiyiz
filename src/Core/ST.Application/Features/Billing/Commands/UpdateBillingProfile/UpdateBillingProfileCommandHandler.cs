using AutoMapper;
using MediatR;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Billing;
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers;
using ST.Domain.Entities.Billing;
using System.Threading;
using System.Threading.Tasks;

namespace ST.Application.Features.Billing.Commands.UpdateBillingProfile
{
    public class UpdateBillingProfileCommandHandler : IRequestHandler<UpdateBillingProfileCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBillingProfileService _billingProfileService;

        public UpdateBillingProfileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBillingProfileService billingProfileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _billingProfileService = billingProfileService;
        }

        public async Task<Response<int>> Handle(UpdateBillingProfileCommand request, CancellationToken cancellationToken)
        {
            var billingProfile = await _billingProfileService.GetBillingProfileByIdAsync(request.Id);

            if (billingProfile == null)
                throw new NotFoundException("Fatura profili bulunamadı.");

            await _billingProfileService.UpdateBillingProfileAsync(_mapper.Map(request, billingProfile));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Response<int>.Success(billingProfile.Id, "Fatura profili başarıyla güncellendi.");
        }
    }
}