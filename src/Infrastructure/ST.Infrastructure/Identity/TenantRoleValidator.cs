using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST.Domain.Entities.Identity;


namespace ST.Infrastructure.Identity
{
    public class TenantRoleValidator<TRole> : RoleValidator<TRole> where TRole : ApplicationRole
    {
        public override async Task<IdentityResult> ValidateAsync(RoleManager<TRole> manager, TRole role)
        {
            string normalizedName = manager.NormalizeKey(role.Name);
            TRole existingRole = await manager.Roles.FirstOrDefaultAsync(r => r.TenantId == role.TenantId && r.NormalizedName == normalizedName);

            if (existingRole != null && existingRole.Id != role.Id)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateRoleName",
                    Description = $"Rol adı '{role.Name}' bu hesap için zaten kullanılıyor."
                });
            }

            return IdentityResult.Success;
        }
    }
}