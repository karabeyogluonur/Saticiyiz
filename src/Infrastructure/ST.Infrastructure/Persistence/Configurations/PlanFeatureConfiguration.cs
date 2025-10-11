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

            builder.HasOne(pf => pf.Plan)
                   .WithMany(p => p.Features)
                   .HasForeignKey(pf => pf.PlanId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pf => pf.FeatureDefinition)
                   .WithMany(fd => fd.PlanFeatures)
                   .HasForeignKey(pf => pf.FeatureDefinitionId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(pf => pf.Value).IsRequired().HasMaxLength(256);
        }
    }
}
