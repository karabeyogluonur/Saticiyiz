using ST.Domain.Entities.Billing;

namespace ST.Application.Interfaces.Billing;

public interface IBillingProfileService
{
    Task<BillingProfile> CreateBillingProfileAsync(BillingProfile billingProfile);
    Task<BillingProfile> GetBillingProfileByIdAsync(int billingProfileId);
    Task<IEnumerable<BillingProfile>> GetAllBillingProfileAsync();
    Task DeleteBillingProfileAsync(BillingProfile billingProfile);
    Task UpdateBillingProfileAsync(BillingProfile billingProfile);

}