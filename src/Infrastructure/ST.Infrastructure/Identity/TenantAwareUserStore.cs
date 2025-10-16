using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities.Identity;
using ST.Infrastructure.Persistence.Contexts;

namespace ST.Infrastructure.Identity;

public class TenantAwareUserStore : UserStore<ApplicationUser, ApplicationRole, SharedDbContext, int>
{
    private readonly ICurrentTenantStore _currentTenantStore;

    public TenantAwareUserStore(SharedDbContext context, ICurrentTenantStore currentTenantStore)
        : base(context)
    {
        _currentTenantStore = currentTenantStore;
    }

    public override IQueryable<ApplicationUser> Users => base.Users.Where(u => u.TenantId == _currentTenantStore.Id);

    // 🔹 Kullanıcı arama işlemleri tenant bazlı hale gelir:
    public override Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        return Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public override Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    // 🔹 Kullanıcı rolleri çekilirken tenant’a göre filtrele:
    public async override Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        return await (from userRole in Context.UserRoles
                      join role in Context.Roles on userRole.RoleId equals role.Id
                      where userRole.UserId.Equals(user.Id)
                            && role.TenantId == _currentTenantStore.Id
                      select role.Name).ToListAsync(cancellationToken);
    }
    public override async Task<bool> IsInRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var role = await Context.Set<ApplicationRole>()
            .SingleOrDefaultAsync(r => r.NormalizedName == normalizedRoleName && r.TenantId == user.TenantId, cancellationToken);

        if (role == null)
            return false;

        var userRole = await Context.Set<IdentityUserRole<int>>()
            .AnyAsync(ur => ur.RoleId == role.Id && ur.UserId == user.Id, cancellationToken);

        return userRole;
    }
}
