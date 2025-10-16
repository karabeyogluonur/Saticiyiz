using ST.Application.Interfaces.Billing;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace ST.Infrastructure.Services.Billing;


public class BillingProfileService : IBillingProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public BillingProfileService(IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<BillingProfile> CreateBillingProfileAsync(BillingProfile billingProfile)
    {
        billingProfile.CreatedDate = DateTime.UtcNow;
        billingProfile.CreatedBy = _userContext.EmailOrUsername;
        billingProfile.TenantId = _userContext.TenantId;

        await _unitOfWork.BillingProfiles.InsertAsync(billingProfile);

        return billingProfile;
    }

    public async Task DeleteBillingProfileAsync(BillingProfile billingProfile)
    {
        billingProfile.DeletedBy = _userContext.EmailOrUsername;
        billingProfile.DeletedDate = DateTime.UtcNow;
        billingProfile.IsDeleted = true;

        _unitOfWork.BillingProfiles.Update(billingProfile);
    }

    public async Task<IEnumerable<BillingProfile>> GetAllBillingProfileAsync()
    {
        return await _unitOfWork.BillingProfiles.GetAllAsync(predicate: billingProfile => !billingProfile.IsDeleted, include: source => source
                        .Include(billingProfile => billingProfile.City)
                        .Include(billingProfile => billingProfile.District));
    }

    public async Task<BillingProfile> GetBillingProfileByIdAsync(int billingProfileId)
    {
        return await _unitOfWork.BillingProfiles.FindAsync(billingProfileId);
    }

    public async Task UpdateBillingProfileAsync(BillingProfile billingProfile)
    {
        billingProfile.LastModifiedBy = _userContext.EmailOrUsername;
        billingProfile.LastModifiedDate = DateTime.UtcNow;

        _unitOfWork.BillingProfiles.Update(billingProfile);
    }
}