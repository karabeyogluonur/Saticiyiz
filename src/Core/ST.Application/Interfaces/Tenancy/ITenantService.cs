using ST.Domain.Entities;
using ST.Domain.Enums;

namespace ST.Application.Interfaces.Tenancy
{

    public interface ITenantService
    {
        Task<ApplicationTenant> CreateTenantAsync(string tenantName);
        Task<ApplicationTenant> GetTenantByIdAsync(int tenantId);
    }
}