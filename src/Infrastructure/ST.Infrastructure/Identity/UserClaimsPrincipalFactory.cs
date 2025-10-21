using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities.Identity;

namespace ST.Infrastructure.Identity;

public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly ITenantService _tenantService;
    public AppClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        ITenantService tenantService)
        : base(userManager, roleManager, optionsAccessor)
    {
        _tenantService = tenantService;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var tenant = await _tenantService.GetTenantByIdAsync(user.TenantId);

        identity.AddClaim(new Claim(CustomClaims.FullName, user.FirstName + " " + user.LastName));
        identity.AddClaim(new Claim(CustomClaims.EmailVerification, user.EmailConfirmed.ToString()));
        identity.AddClaim(new Claim(CustomClaims.TenantId, user.TenantId.ToString()));
        identity.AddClaim(new Claim(CustomClaims.IsSetupCompleted, tenant.IsSetupCompleted.ToString()));
        return identity;
    }
}
