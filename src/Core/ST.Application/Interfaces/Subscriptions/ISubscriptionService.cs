using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Enums;

namespace ST.Application.Interfaces.Subscriptions;

public interface ISubscriptionService
{
    Task<Subscription> AssignTrialSubscriptionAsync(int tenantId, int planId, int trialDays);
    Task<Subscription> GetActiveOrTrialSubscriptionAsync(int tenantId);
    Task<Subscription> GetActiveOrTrialSubscriptionAsync();
}
