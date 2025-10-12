
using MediatR;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities;
using ST.Domain.Tenancy;

namespace ST.Application.Features.Tenancy.EventHandlers;

public class SeedDefaultRolesForTenantEventHandler : INotificationHandler<TenantCreatedEvent>
{
    private readonly IRoleService _roleService;
    private readonly ITenantService _tenantService;

    public SeedDefaultRolesForTenantEventHandler(IRoleService roleService, ITenantService tenantService)
    {
        _roleService = roleService;
        _tenantService = tenantService;
    }

    public async Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
    {
        ApplicationTenant tenant = await _tenantService.GetTenantByIdAsync(notification.TenantId);

        if (tenant is null)
            return;

        await _roleService.SeedDefaultRolesForTenantAsync(tenant);
    }
}
