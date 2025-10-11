namespace ST.Application.Interfaces.Subscriptions;

public interface IFeatureAccessService
{
    Task EnforceFeatureAccessAsync(string featureKey);
    Task<T> GetFeatureValueAsync<T>(string featureKey);
}
