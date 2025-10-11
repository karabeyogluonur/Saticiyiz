
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities.Subscriptions;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();

            builder.HasMany(p => p.Features)
                   .WithOne(pf => pf.Plan)
                   .HasForeignKey(pf => pf.PlanId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
