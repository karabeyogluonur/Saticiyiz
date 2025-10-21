using ST.Domain.Entities.Subscriptions;

namespace ST.Application.Interfaces.Subscriptions;

public interface IPlanService
{
    public Task<Plan> GetDefaultPlanAsync();
    public Task<Plan> GetTrialPlanAsync();
}
