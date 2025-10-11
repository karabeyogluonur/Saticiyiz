using MediatR;
using ST.Application.Interfaces.Subscriptions;
using System.Reflection;

namespace ST.Application.Common.Behaviors;

public class PlanCheckPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IFeatureAccessService _featureAccessService;

    public PlanCheckPipelineBehavior(IFeatureAccessService featureAccessService)
    {
        _featureAccessService = featureAccessService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        IEnumerable<RequiresPlanFeatureAttribute> requiredFeatures = typeof(TRequest).GetCustomAttributes<RequiresPlanFeatureAttribute>();

        foreach (RequiresPlanFeatureAttribute feature in requiredFeatures)
        {
            await _featureAccessService.EnforceFeatureAccessAsync(feature.FeatureKey);
        }

        return await next();
    }
}
