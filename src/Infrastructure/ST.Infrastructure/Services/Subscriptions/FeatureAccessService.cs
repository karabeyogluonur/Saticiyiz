using ST.Application.Exceptions;
using ST.Application.Interfaces.Subscriptions;
using ST.Domain.Enums;
using ST.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Finbuckle.MultiTenant;
using System.Globalization;
using ST.Domain.Entities.Subscriptions;
using ST.Infrastructure.Persistence.Contexts;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities;

namespace ST.Infrastructure.Services.Subscriptions;

public class FeatureAccessService : IFeatureAccessService
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<FeatureAccessService> _logger;

    public FeatureAccessService(
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FeatureAccessService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task EnforceFeatureAccessAsync(string featureKey)
    {
        var hasAccess = await GetFeatureValueAsync<bool>(featureKey);
        if (!hasAccess)
        {
            _logger.LogWarning("Tenant '{TenantId}' attempted to access forbidden feature '{FeatureKey}'.",
                GetCurrentTenantId(), featureKey);

            throw new ForbiddenAccessException($"Your current plan does not allow access to this feature: {featureKey}.");
        }
    }

    /// <inheritdoc />
    public async Task<T> GetFeatureValueAsync<T>(string featureKey)
    {
        var tenantId = GetCurrentTenantId();

        if (tenantId is null)
        {
            _logger.LogCritical("Feature access attempted without a valid tenant context.");
            throw new UnauthorizedAccessException("Cannot determine feature access without tenant context.");
        }

        var planId = await GetCurrentPlanIdAsync(tenantId);

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

    private async Task<int?> GetCurrentPlanIdAsync(string tenantId)
    {
        var subscriptionRepository = _unitOfWork.GetRepository<Subscription>();

        var subscription = await subscriptionRepository.GetAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active);

        return subscription?.PlanId;
    }

    private async Task<Dictionary<string, string>> GetPlanFeaturesAsync(int planId)
    {
        var planFeatureRepository = _unitOfWork.GetRepository<PlanFeature>();

        return await planFeatureRepository.GetAll()
            .AsNoTracking()
            .Include(pf => pf.FeatureDefinition)
            .Where(pf => pf.PlanId == planId)
            .ToDictionaryAsync(pf => pf.FeatureDefinition.Key, pf => pf.Value);
    }

    private string? GetCurrentTenantId()
    {
        return _httpContextAccessor.HttpContext?.GetMultiTenantContext<ApplicationTenant>()?.TenantInfo?.Id;
    }
}