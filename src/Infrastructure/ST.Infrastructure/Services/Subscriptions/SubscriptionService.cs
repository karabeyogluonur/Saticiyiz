using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Subscriptions;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Enums;
using System.Globalization;

namespace ST.Infrastructure.Services.Subscriptions;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingService _settingService;

    public SubscriptionService(IUnitOfWork unitOfWork, ISettingService settingService)
    {
        _unitOfWork = unitOfWork;
        _settingService = settingService;
    }

    public async Task<Subscription> AssignTrialSubscriptionAsync(ApplicationTenant tenant, Plan plan, int trialDays)
    {
        var subscription = new Subscription
        {
            Tenant = tenant,
            Plan = plan,
            Status = SubscriptionStatus.Trial,
            CurrentPeriodEndDate = DateTime.UtcNow.AddDays(trialDays),
            CreatedBy = "System",
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Subscriptions.InsertAsync(subscription);

        return subscription;
    }

    public async Task<Subscription> GetActiveOrTrialSubscriptionAsync(int tenantId)
    {
        return await _unitOfWork.Subscriptions
        .GetFirstOrDefaultAsync(predicate: s => s.TenantId == tenantId &&
                                (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial), ignoreQueryFilters: true);
    }

    public async Task<Subscription> GetActiveOrTrialSubscriptionAsync()
    {
        return await _unitOfWork.Subscriptions
        .GetFirstOrDefaultAsync(predicate: s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial);
    }
}
