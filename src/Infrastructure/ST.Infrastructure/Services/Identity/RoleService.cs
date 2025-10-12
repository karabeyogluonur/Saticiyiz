using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ST.Application.Common.Constants;
using ST.Application.DTOs.Messages;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Messages;
using ST.Application.Interfaces.Repositories;
using ST.Application.Settings;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;
using ST.Domain.Enums;
using ST.Infrastructure.Services.Email;


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
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }
    }

    public async Task SeedDefaultRolesForTenantAsync(ApplicationTenant tenant)
    {
        IList<ApplicationRole> tenantRoles = await _roleManager.Roles.AsNoTracking().Where(role => role.TenantId == tenant.Id).ToListAsync();

        List<string> defaultRoles = await GetDefaultRolesAsync();

        if (defaultRoles is null)
            return;

        var seedTasks = defaultRoles
            .Where(defaultRole => !tenantRoles.Any(tenantRole => tenantRole.Name == defaultRole))
            .Select(roleName =>
            {
                var newRole = new ApplicationRole
                {
                    Name = roleName,
                    TenantId = tenant.Id,
                    Description = $"Organizasyon için varsayılan {roleName} rolü."
                };
                return _roleManager.CreateAsync(newRole);
            });

        await Task.WhenAll(seedTasks);
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
