using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Contexts;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities.Common;
using ST.Domain.Entities.Identity;
using ST.Domain.Events.Common;
using System.Linq.Expressions;
using ST.Domain.Entities.Billing;
using ST.Domain.Entities.Configurations;
using ST.Domain.Entities.Lookup;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Entities;

namespace ST.Infrastructure.Persistence.Contexts
{
    public class SharedDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, ISharedDbContext
    {
        private readonly List<DomainEvent> _domainEvents = new();
        public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        private readonly ICurrentTenantStore _currentTenantStore;
        public DbSet<ApplicationTenant> ApplicationTenants { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<FeatureDefinition> FeatureDefinitions { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<BillingProfile> BillingProfiles { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }

        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {
            // Bu constructor, ICurrentTenantStore olmadan çağrıldığında (örneğin migration oluştururken)
            // _currentTenantStore null kalacaktır. Bu bir sorun teşkil etmez.
        }

        public SharedDbContext(
            DbContextOptions<SharedDbContext> options,
            ICurrentTenantStore currentTenantStore) : base(options)
        {
            _currentTenantStore = currentTenantStore;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // ÇÖZÜM: Filtre ifadesini, sorgu anında ICurrentTenantStore.Id'yi
                    // okuyacak şekilde dinamik hale getiriyoruz.
                    builder.Entity(entityType.ClrType)
                           .HasQueryFilter(CreateTenantFilter(entityType.ClrType));
                }
            }

            // Setting için özel filtreleme.
            // Bu da artık sorgu anındaki _currentTenantStore.Id'yi kullanacak.
            builder.Entity<Setting>().HasQueryFilter(e => e.TenantId == _currentTenantStore.Id || e.TenantId == null);

            builder.ApplyConfigurationsFromAssembly(typeof(SharedDbContext).Assembly);
        }

        private LambdaExpression CreateTenantFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            // e.TenantId
            var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

            // this._currentTenantStore.Id
            var tenantStoreField = typeof(SharedDbContext).GetField(nameof(_currentTenantStore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tenantStoreExpression = Expression.Field(Expression.Constant(this), tenantStoreField!);
            var tenantIdProperty = typeof(ICurrentTenantStore).GetProperty(nameof(ICurrentTenantStore.Id));
            var tenantIdExpression = Expression.Property(tenantStoreExpression, tenantIdProperty!);

            // e.TenantId == this._currentTenantStore.Id
            // NOT: Her iki tarafın da tipleri (int ve int?) uyuşmadığı için Convert kullanıyoruz.
            var body = Expression.Equal(
                property,
                Expression.Convert(tenantIdExpression, property.Type));

            // _currentTenantStore'un null olmadığı bir senaryo için ek kontrol
            var nullCheck = Expression.NotEqual(tenantStoreExpression, Expression.Constant(null));
            var finalBody = Expression.AndAlso(nullCheck, body);

            return Expression.Lambda(finalBody, parameter);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            CollectDomainEvents();

            if (_currentTenantStore?.Id.HasValue == true)
            {
                foreach (var entry in ChangeTracker.Entries<ITenantEntity>().Where(e => e.State == EntityState.Added))
                {
                    if (entry.Entity.TenantId == 0)
                    {
                        entry.Entity.TenantId = _currentTenantStore.Id.Value;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void CollectDomainEvents()
        {
            var domainEventEntities = ChangeTracker.Entries<IDomainEvent>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            foreach (var entity in domainEventEntities)
            {
                _domainEvents.AddRange(entity.DomainEvents);
                entity.ClearDomainEvents();
            }
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}