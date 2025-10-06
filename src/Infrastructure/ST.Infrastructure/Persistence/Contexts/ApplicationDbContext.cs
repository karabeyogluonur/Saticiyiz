using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;
using ST.Infrastructure.Identity;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using ST.Domain.Entities.Configurations;
using Microsoft.AspNetCore.Identity;

namespace ST.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IMultiTenantContextAccessor _tenantContextAccessor;
        private readonly string? _currentTenantId;

        public DbSet<ApplicationTenant> ApplicationTenants { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<FeatureDefinition> FeatureDefinitions { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            _currentTenantId = null;
        }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IMultiTenantContextAccessor tenantContextAccessor)
            : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _currentTenantId = tenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserClaim<string>>().IsMultiTenant().AdjustUniqueIndexes();
            builder.Entity<IdentityUserRole<string>>().IsMultiTenant().AdjustUniqueIndexes();
            builder.Entity<IdentityUserLogin<string>>().IsMultiTenant().AdjustUniqueIndexes();
            builder.Entity<IdentityRoleClaim<string>>().IsMultiTenant().AdjustUniqueIndexes();
            builder.Entity<IdentityUserToken<string>>().IsMultiTenant().AdjustUniqueIndexes();

            builder.ConfigureMultiTenant();

            if (_currentTenantId != null)
            {
                builder.Entity<Subscription>()
                    .HasQueryFilter(s => s.TenantId == _currentTenantId);

                builder.Entity<ApplicationUser>()
                    .HasQueryFilter(u => u.TenantId == _currentTenantId);
            }

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
