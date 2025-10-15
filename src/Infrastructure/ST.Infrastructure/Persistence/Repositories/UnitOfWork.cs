using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Entities.Billing;
using ST.Domain.Entities.Lookup;
using ST.Domain.Events.Common;
using ST.Infrastructure.Persistence.Contexts;
using System.Transactions;
using ST.Domain.Entities.Configurations;

namespace ST.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SharedDbContext _sharedContext;
        private readonly TenantDbContext _tenantContext;

        private IUserRepository _users;
        private IRoleRepository _roles;
        private ITenantRepository _tenants;
        private ISubscriptionRepository _subscriptions;
        private ISettingRepository _settings;
        private IPlanRepository _plans;
        private IPlanFeatureRepository _planFeatures;
        private IFeatureDefinitionRepository _featureDefinitions;
        private IBillingProfileRepository _billingProfiles;
        private ICityRepository _cities;
        private IDistrictRepository _districts;

        public UnitOfWork(SharedDbContext sharedContext, TenantDbContext tenantContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public IUserRepository Users => _users ??= new UserRepository(_sharedContext);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_sharedContext);
        public ITenantRepository Tenants => _tenants ??= new TenantRepository(_sharedContext);
        public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_sharedContext);

        public ISettingRepository Settings => _settings ??= new SettingRepository(_sharedContext);

        public IPlanRepository Plans => _plans ??= new PlanRepository(_sharedContext);

        public IPlanFeatureRepository PlanFeatures => _planFeatures ??= new PlanFeatureRepository(_sharedContext);

        public IFeatureDefinitionRepository FeatureDefinitions => _featureDefinitions ??= new FeatureDefinitionRepository(_sharedContext);

        public IBillingProfileRepository BillingProfiles => _billingProfiles ??= new BillingProfileRepository(_sharedContext);

        public ICityRepository Cities => _cities ??= new CityRepository(_sharedContext);

        public IDistrictRepository Districts => _districts ??= new DistrictRepository(_sharedContext);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var sharedResult = await _sharedContext.SaveChangesAsync(cancellationToken);
            var tenantResult = 0;
            if (_tenantContext.ChangeTracker.HasChanges())
            {
                tenantResult = await _tenantContext.SaveChangesAsync(cancellationToken);
            }

            return sharedResult + tenantResult;
        }

        public void Dispose()
        {
            _sharedContext.Dispose();
            _tenantContext.Dispose();
            GC.SuppressFinalize(this);
        }

        public IReadOnlyList<DomainEvent> GetDomainEvents()
        {
            var events = _sharedContext.DomainEvents.ToList();
            return events;
        }

        public void ClearDomainEvents()
        {
            _sharedContext.ClearDomainEvents();
        }
    }

    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        public UserRepository(SharedDbContext context) : base(context) { }
    }
    public class RoleRepository : Repository<ApplicationRole>, IRoleRepository
    {
        public RoleRepository(SharedDbContext context) : base(context) { }
    }
    public class TenantRepository : Repository<ApplicationTenant>, ITenantRepository
    {
        public TenantRepository(SharedDbContext context) : base(context) { }
    }
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(SharedDbContext context) : base(context) { }
    }
    public class SettingRepository : Repository<Setting>, ISettingRepository
    {
        public SettingRepository(SharedDbContext context) : base(context) { }
    }
    public class PlanRepository : Repository<Plan>, IPlanRepository
    {
        public PlanRepository(SharedDbContext context) : base(context) { }
    }
    public class PlanFeatureRepository : Repository<PlanFeature>, IPlanFeatureRepository
    {
        public PlanFeatureRepository(SharedDbContext context) : base(context) { }
    }
    public class FeatureDefinitionRepository : Repository<FeatureDefinition>, IFeatureDefinitionRepository
    {
        public FeatureDefinitionRepository(SharedDbContext context) : base(context) { }
    }
    public class BillingProfileRepository : Repository<BillingProfile>, IBillingProfileRepository
    {
        public BillingProfileRepository(SharedDbContext context) : base(context) { }
    }
    public class CityRepository : Repository<City>, ICityRepository
    {
        public CityRepository(SharedDbContext context) : base(context) { }
    }
    public class DistrictRepository : Repository<District>, IDistrictRepository
    {
        public DistrictRepository(SharedDbContext context) : base(context) { }
    }
}
