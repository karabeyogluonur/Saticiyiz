using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Entities;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.HasKey(s => s.Id);

            // TenantId FK Konfigürasyonu (Restrict)
            builder.Property(s => s.TenantId).IsRequired().HasMaxLength(64);
            builder.HasOne(s => s.Tenant)
                   .WithMany(t => t.Subscriptions)
                   .HasForeignKey(s => s.TenantId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict); // Abonelik varken Kiracı silinemez.

            // PlanId FK Konfigürasyonu (Restrict)
            builder.HasOne(s => s.Plan)
                   .WithMany(p => p.Subscriptions)
                   .HasForeignKey(s => s.PlanId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict); // Plan varken Abonelik silinemez.

            // Indexler
            builder.HasIndex(s => s.TenantId);
            builder.HasIndex(s => s.PlanId);
        }
    }
}