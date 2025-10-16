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
using ST.Infrastructure.Persistence.Repositories;

namespace ST.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SharedDbContext _sharedContext;
        private readonly TenantDbContext _tenantContext;

        // ÇÖZÜM: Repository'ler için private backing field'lar kaldırıldı.
        // Bu, her property çağrıldığında yeni bir repository örneği oluşturulmasını sağlar.

        public UnitOfWork(SharedDbContext sharedContext, TenantDbContext tenantContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        // Her çağrıldığında, o anki isteğe ait _sharedContext ile YENİ BİR repository oluşturur.
        public IUserRepository Users => new UserRepository(_sharedContext);
        public IRoleRepository Roles => new RoleRepository(_sharedContext);
        public ITenantRepository Tenants => new TenantRepository(_sharedContext);
        public ISubscriptionRepository Subscriptions => new SubscriptionRepository(_sharedContext);
        public ISettingRepository Settings => new SettingRepository(_sharedContext);
        public IPlanRepository Plans => new PlanRepository(_sharedContext);
        public IPlanFeatureRepository PlanFeatures => new PlanFeatureRepository(_sharedContext);
        public IFeatureDefinitionRepository FeatureDefinitions => new FeatureDefinitionRepository(_sharedContext);
        public IBillingProfileRepository BillingProfiles => new BillingProfileRepository(_sharedContext);
        public ICityRepository Cities => new CityRepository(_sharedContext);
        public IDistrictRepository Districts => new DistrictRepository(_sharedContext);

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
