using AutoMapper;
using MediatR;
using ST.Application.Interfaces.Billing;
using ST.Application.Interfaces.Common; // IUserContext için
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers; // Response<T> için
using ST.Domain.Entities.Billing;
using System.Threading;
using System.Threading.Tasks;

namespace ST.Application.Features.Billing.Commands.CreateBillingProfile
{
    public class CreateBillingProfileCommandHandler : IRequestHandler<CreateBillingProfileCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBillingProfileService _billingProfileService;

        public CreateBillingProfileCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBillingProfileService billingProfileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _billingProfileService = billingProfileService;
        }

        public async Task<Response<int>> Handle(CreateBillingProfileCommand request, CancellationToken cancellationToken)
        {
            BillingProfile billingProfile = await _billingProfileService.CreateBillingProfileAsync(_mapper.Map<BillingProfile>(request));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Response<int>.Success(billingProfile.Id, "Fatura profili başarıyla oluşturuldu.");
        }
    }
}