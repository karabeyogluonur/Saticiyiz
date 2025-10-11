using ST.Domain.Entities;
using ST.Domain.Entities.Billing;
using ST.Domain.Entities.Configurations;
using ST.Domain.Entities.Identity;
using ST.Domain.Entities.Lookup;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Events.Common;

namespace ST.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {

        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        ITenantRepository Tenants { get; }
        ISubscriptionRepository Subscriptions { get; }
        ISettingRepository Settings { get; }
        IPlanRepository Plans { get; }
        IPlanFeatureRepository PlanFeatures { get; }
        IFeatureDefinitionRepository FeatureDefinitions { get; }
        IBillingProfileRepository BillingProfiles { get; }
        ICityRepository Cities { get; }
        IDistrictRepository Districts { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        IReadOnlyList<DomainEvent> GetDomainEvents();
        void ClearDomainEvents();
    }

    public interface IUserRepository : IRepository<ApplicationUser> { }
    public interface IRoleRepository : IRepository<ApplicationRole> { }
    public interface ITenantRepository : IRepository<ApplicationTenant> { }
    public interface ISubscriptionRepository : IRepository<Subscription> { }
    public interface ISettingRepository : IRepository<Setting> { }
    public interface IPlanRepository : IRepository<Plan> { }
    public interface IPlanFeatureRepository : IRepository<PlanFeature> { }
    public interface IFeatureDefinitionRepository : IRepository<FeatureDefinition> { }
    public interface IBillingProfileRepository : IRepository<BillingProfile> { }
    public interface ICityRepository : IRepository<City> { }
    public interface IDistrictRepository : IRepository<District> { }


}
