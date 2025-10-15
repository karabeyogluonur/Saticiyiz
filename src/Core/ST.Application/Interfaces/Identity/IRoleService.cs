using Microsoft.AspNetCore.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;

namespace ST.Application.Interfaces.Identity
{
    public interface IRoleService
    {
        Task SeedDefaultRolesForTenantAsync(int tenantId);
        Task AssignRoleToUserAsync(ApplicationUser user, string roleName);
    }
}
