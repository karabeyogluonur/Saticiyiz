using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;


namespace ST.Infrastructure.Services.Identity;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    public RoleService(IUnitOfWork unitOfWork, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task AssignRoleToUserAsync(ApplicationUser user, string roleName)
    {
        await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task SeedDefaultRolesForTenantAsync(int tenantId)
    {

        var tenantRoles = await _roleManager.Roles
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId)
            .Select(r => r.Name)
            .ToListAsync();

        var defaultRoles = await GetDefaultRolesAsync();

        foreach (var roleName in defaultRoles)
        {
            if (!tenantRoles.Contains(roleName))
            {
                var newRole = new ApplicationRole()
                {
                    Name = roleName,
                    Description = $"Organizasyon için varsayılan {roleName} rolü.",
                    TenantId = tenantId
                };

                IdentityResult result = await _roleManager.CreateAsync(newRole);
                Console.WriteLine($"{result.Succeeded} --------------");
            }
        }
    }

    private async Task<List<string>> GetDefaultRolesAsync()
    {
        return typeof(AppRoles).GetFields(System.Reflection.BindingFlags.Public |
                                                        System.Reflection.BindingFlags.Static |
                                                        System.Reflection.BindingFlags.FlattenHierarchy)
                                                        .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                                                        .Select(fi => fi.GetRawConstantValue()?.ToString())
                                                        .ToList();
    }
}
