using Microsoft.EntityFrameworkCore;
using ST.Domain.Entities;
using ST.Domain.Entities.Billing;
using ST.Domain.Entities.Configurations;
using ST.Domain.Entities.Lookup;
using ST.Domain.Entities.Subscriptions;

namespace ST.Application.Interfaces.Contexts
{
    /// <summary>
    /// Ortak veritabanında bulunan tüm varlıklar için bir sözleşme tanımlar.
    /// </summary>
    public interface ISharedDbContext
    {
        // Temel ve Ortak Varlıklar
        DbSet<ApplicationTenant> ApplicationTenants { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        DbSet<Plan> Plans { get; set; }
        DbSet<FeatureDefinition> FeatureDefinitions { get; set; }
        DbSet<PlanFeature> PlanFeatures { get; set; }
        DbSet<City> Cities { get; set; }
        DbSet<District> Districts { get; set; }
        DbSet<Setting> Settings { get; set; }
        DbSet<BillingProfile> BillingProfiles { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}