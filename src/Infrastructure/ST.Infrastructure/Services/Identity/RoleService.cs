using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Identity;
using ST.Domain.Entities.Identity;

namespace ST.Infrastructure.Services.Identity
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task CreateDefaultRolesForTenantAsync(string tenantId)
        {
            await TryCreateRoleAsync(AppRoles.TenantOwner, tenantId);
            await TryCreateRoleAsync(AppRoles.TenantMember, tenantId);
        }

        private async Task TryCreateRoleAsync(string roleName, string tenantId)
        {
            try
            {
                string normalizedName = _roleManager.NormalizeKey(roleName);
                bool roleExists = await _roleManager.Roles.IgnoreQueryFilters()
                    .AnyAsync(r => r.NormalizedName == normalizedName && r.TenantId == tenantId);

                if (!roleExists)
                {
                    IdentityResult result = await _roleManager.CreateAsync(new ApplicationRole()
                    {
                        Name = roleName,
                        TenantId = tenantId
                    });
                }
            }
            catch (DbUpdateException)
            {
            }
        }
    }
}