using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST.Application.Interfaces.Contexts;
using ST.Application.Interfaces.Tenancy;
using ST.Domain.Entities;
using ST.Domain.Entities.Billing;
using ST.Domain.Entities.Common;
using ST.Domain.Entities.Configurations;
using ST.Domain.Entities.Identity;
using ST.Domain.Entities.Lookup;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Events.Common;
using System.Linq.Expressions;

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


        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options) { }

        public SharedDbContext(
            DbContextOptions<SharedDbContext> options,
            ICurrentTenantStore currentTenantStore) : base(options)
        {
            _currentTenantStore = currentTenantStore;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            if (_currentTenantStore != null && _currentTenantStore.Id.HasValue)
            {
                foreach (var entityType in builder.Model.GetEntityTypes())
                {
                    if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var body = Expression.Equal(
                            Expression.Property(parameter, nameof(ITenantEntity.TenantId)),
                            Expression.Constant(_currentTenantStore.Id.Value));
                        builder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
                    }
                }
            }

            builder.Entity<Setting>().HasQueryFilter(e => e.TenantId == _currentTenantStore.Id || e.TenantId == null);

            builder.ApplyConfigurationsFromAssembly(typeof(SharedDbContext).Assembly);
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

            int result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }

        private void CollectDomainEvents()
        {
            var domainEventEntities = ChangeTracker.Entries<IDomainEvent>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            // Event'leri IMediator'a göndermek yerine, sadece bu context'e ait listeye ekliyoruz.
            foreach (var entity in domainEventEntities)
            {
                _domainEvents.AddRange(entity.DomainEvents);
                entity.ClearDomainEvents();
            }
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
