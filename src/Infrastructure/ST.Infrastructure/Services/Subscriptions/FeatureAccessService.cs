using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ST.Application.Exceptions;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Subscriptions;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Enums;
using System.Globalization;

namespace ST.Infrastructure.Services.Subscriptions;

public class FeatureAccessService : IFeatureAccessService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantStore _currentTenantStore;
    private readonly ILogger<FeatureAccessService> _logger;

    public FeatureAccessService(
        IUnitOfWork unitOfWork,
        ICurrentTenantStore currentTenantStore,
        ILogger<FeatureAccessService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentTenantStore = currentTenantStore;
        _logger = logger;
    }

    public async Task EnforceFeatureAccessAsync(string featureKey)
    {
        var hasAccess = await GetFeatureValueAsync<bool>(featureKey);
        if (!hasAccess)
        {
            _logger.LogWarning("Tenant '{TenantId}' attempted to access forbidden feature '{FeatureKey}'.",
                _currentTenantStore.Id, featureKey);

            throw new ForbiddenAccessException($"Your current plan does not allow access to this feature: {featureKey}.");
        }
    }

    public async Task<T> GetFeatureValueAsync<T>(string featureKey)
    {
        if (_currentTenantStore.Id is null)
        {
            _logger.LogCritical("Feature access attempted without a valid tenant context.");
            throw new UnauthorizedAccessException("Cannot determine feature access without tenant context.");
        }

        var tenantId = _currentTenantStore.Id.Value;

        var planId = await GetCurrentPlanIdAsync();

        if (planId is null)
        {
            _logger.LogWarning("No active subscription found for TenantId '{TenantId}'. Access restricted.", tenantId);
            return default!;
        }

        var planFeatures = await GetPlanFeaturesAsync(planId.Value);

        if (planFeatures.TryGetValue(featureKey, out var stringValue))
        {
            try
            {
                return (T)Convert.ChangeType(stringValue, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert feature value. Key: {Key}, Value: {Value}, TargetType: {TargetType}",
                    featureKey, stringValue, typeof(T).Name);

                return default!;
            }
        }

        return default!;
    }

    private async Task<int?> GetCurrentPlanIdAsync()
    {

        var subscription = await _unitOfWork.Subscriptions.GetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Status == SubscriptionStatus.Active);

        return subscription?.PlanId;
    }

    private async Task<Dictionary<string, string>> GetPlanFeaturesAsync(int planId)
    {
        return await _unitOfWork.PlanFeatures.GetAll()
            .AsNoTracking()
            .Include(pf => pf.FeatureDefinition)
            .Where(pf => pf.PlanId == planId)
            .ToDictionaryAsync(pf => pf.FeatureDefinition.Key, pf => pf.Value);
    }
}
