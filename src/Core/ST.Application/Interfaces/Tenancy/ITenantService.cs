using ST.Application.Wrappers;
using ST.Domain.Entities;

namespace ST.Application.Interfaces.Tenancy
{
    public interface ITenantService
    {
        Task<Response<ApplicationTenant>> CreateTenantAsync();
    }
}