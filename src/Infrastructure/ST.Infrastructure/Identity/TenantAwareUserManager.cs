using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ST.Domain.Entities.Identity;
using ST.Infrastructure.Persistence.Contexts;

namespace ST.Infrastructure.Identity;

public class TenantAwareUserManager : UserManager<ApplicationUser>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SharedDbContext _sharedDbContext;

    public TenantAwareUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger,
        RoleManager<ApplicationRole> roleManager,
        SharedDbContext sharedDbContext)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _roleManager = roleManager;
        _sharedDbContext = sharedDbContext;
    }

    public override async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName)
    {
        var normalizedRole = NormalizeName(roleName);

        var role = await _roleManager.Roles
            .SingleOrDefaultAsync(r => r.NormalizedName == normalizedRole && r.TenantId == user.TenantId);

        if (role == null)
            return IdentityResult.Failed(new IdentityError { Description = $"Role '{roleName}' not found for tenant {user.TenantId}" });

        var userRole = new IdentityUserRole<int>
        {
            RoleId = role.Id,
            UserId = user.Id
        };

        _sharedDbContext.Set<IdentityUserRole<int>>().Add(userRole);
        await _sharedDbContext.SaveChangesAsync();

        return IdentityResult.Success;
    }
}
