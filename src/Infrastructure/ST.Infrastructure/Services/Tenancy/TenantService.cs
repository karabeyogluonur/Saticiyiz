using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities;

namespace ST.Infrastructure.Services.Tenancy;

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplicationTenant> CreateTenantAsync(string tenantName)
    {
        ApplicationTenant applicationTenant = new ApplicationTenant
        {
            Name = tenantName,
            Identifier = Guid.NewGuid().ToString(),
            CreatedBy = "System",
        };

        await _unitOfWork.Tenants.InsertAsync(applicationTenant);

        return applicationTenant;

    }
    public async Task<ApplicationTenant> GetTenantByIdAsync(int tenantId)
    {
        return await _unitOfWork.Tenants.GetFirstOrDefaultAsync(predicate: t => t.Id == tenantId);
    }

    public async Task MarkSetupAsCompletedAsync(ApplicationTenant tenant)
    {
        tenant.IsSetupCompleted = true;
        tenant.LastModifiedBy = "System";
        tenant.LastModifiedDate = DateTime.UtcNow;

        _unitOfWork.Tenants.Update(tenant);
    }
}