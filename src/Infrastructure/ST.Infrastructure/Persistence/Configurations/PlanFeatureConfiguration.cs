using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
    {
        public void Configure(EntityTypeBuilder<PlanFeature> builder)
        {
            builder.HasKey(pf => pf.Id);

            // FK Tanımı 1: Plan -> PlanFeature
            builder.HasOne(pf => pf.Plan)
                   .WithMany(p => p.Features) // Plan Entity'deki Features koleksiyonu
                   .HasForeignKey(pf => pf.PlanId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); // Plan silinirse özellikler silinsin.

            // FK Tanımı 2: FeatureDefinition -> PlanFeature
            builder.HasOne(pf => pf.FeatureDefinition)
                   .WithMany(fd => fd.PlanFeatures) // FeatureDefinition Entity'deki PlanFeatures koleksiyonu
                   .HasForeignKey(pf => pf.FeatureDefinitionId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); // Özellik tanımı silinirse ilişki silinsin.

            // Value ve diğer alanların kısıtlamaları (Eğer varsa)
            builder.Property(pf => pf.Value).IsRequired().HasMaxLength(256);
        }
    }
}