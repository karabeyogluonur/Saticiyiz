using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using ST.Domain.Entities.Identity;
using MediatR;
using ST.Domain.Events.Common;
using ST.Domain.Entities.Configurations;

namespace ST.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        private readonly IMultiTenantContextAccessor _tenantContextAccessor;
        private readonly IMediator _mediator;
        private readonly string? _currentTenantId;

        public DbSet<ApplicationTenant> ApplicationTenants { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<FeatureDefinition> FeatureDefinitions { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            _currentTenantId = null;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMultiTenantContextAccessor tenantContextAccessor, IMediator mediator) : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _currentTenantId = tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;
            _mediator = mediator;
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserLogin<int>>().IsMultiTenant().AdjustUniqueIndexes();
            builder.Entity<IdentityRoleClaim<int>>().IsMultiTenant().AdjustUniqueIndexes();
            builder.Entity<IdentityUserToken<int>>().IsMultiTenant().AdjustUniqueIndexes();

            builder.ConfigureMultiTenant();

            if (_currentTenantId != null)
            {
                builder.Entity<Subscription>().HasQueryFilter(s => s.TenantId == _currentTenantId);
                builder.Entity<ApplicationUser>().HasQueryFilter(u => u.TenantId == _currentTenantId);
            }

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            int result = await base.SaveChangesAsync(cancellationToken);
            await _dispatchDomainEvents();
            return result;
        }

        private async Task _dispatchDomainEvents()
        {
            List<IDomainEvent> domainEventEntities = ChangeTracker.Entries<IDomainEvent>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            foreach (IDomainEvent entity in domainEventEntities)
            {
                List<DomainEvent> events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                foreach (DomainEvent domainEvent in events)
                {
                    await _mediator.Publish(domainEvent);
                }
            }
        }
    }
}
