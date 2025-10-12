
using MediatR;
using Microsoft.Extensions.Logging;
using ST.Application.Common.Helpers;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Subscriptions;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Settings;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Tenancy;

namespace ST.Application.Features.Tenancy.EventHandlers;

public class AssignTrialSubscriptionEventHandler : INotificationHandler<TenantCreatedEvent>
{
    private readonly ITenantService _tenantService;
    private readonly IPlanService _planService;
    private readonly ISettingService _settingService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<AssignTrialSubscriptionEventHandler> _logger;

    public AssignTrialSubscriptionEventHandler(IPlanService planService,
                                                ITenantService tenantService,
                                                ISettingService settingService,
                                                ISubscriptionService subscriptionService,
                                                ILogger<AssignTrialSubscriptionEventHandler> logger)
    {
        _tenantService = tenantService;
        _planService = planService;
        _settingService = settingService;
        _subscriptionService = subscriptionService;
        _logger = logger;

    }

    public async Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
    {
        int trialDays = await _settingService.GetValueAsync<int>(SettingKeyHelper.GetKey((SubscriptionSetting s) => s.TrialPeriodDays));

        if (trialDays <= 0)
            throw new InvalidOperationException("Trial days setting is not found.");

        Plan defaultPlan = await _planService.GetDefaultPlanAsync();

        if (defaultPlan == null)
            throw new InvalidOperationException("Default plan not found for tenant trial assignment.");

        Subscription subscription = await _subscriptionService.GetActiveOrTrialSubscriptionAsync(notification.TenantId);

        if (subscription is null)
        {
            ApplicationTenant tenant = await _tenantService.GetTenantByIdAsync(notification.TenantId);
            await _subscriptionService.AssignTrialSubscriptionAsync(tenant, defaultPlan, trialDays);
        }
    }
}
