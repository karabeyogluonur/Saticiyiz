using MediatR;
using ST.Application.Exceptions;
using ST.Application.Features.Billing.Commands.DeleteBillingProfile;
using ST.Application.Interfaces.Billing;
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers;
using ST.Domain.Entities.Billing;
using System.Threading;
using System.Threading.Tasks;

namespace ST.Application.Features.Billing.Commands
{
    public class DeleteBillingProfileCommandHandler : IRequestHandler<DeleteBillingProfileCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBillingProfileService _billingProfileService;

        public DeleteBillingProfileCommandHandler(IUnitOfWork unitOfWork, IBillingProfileService billingProfileService)
        {
            _unitOfWork = unitOfWork;
            _billingProfileService = billingProfileService;
        }

        public async Task<Response<int>> Handle(DeleteBillingProfileCommand request, CancellationToken cancellationToken)
        {
            BillingProfile billingProfile = await _billingProfileService.GetBillingProfileByIdAsync(request.Id);

            if (billingProfile == null)
                throw new NotFoundException("Fatura profili bulunamadı.");

            await _billingProfileService.DeleteBillingProfileAsync(billingProfile);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Response<int>.Success(billingProfile.Id, "Fatura profili başarıyla silindi.");
        }
    }
}