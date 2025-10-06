using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities;
using ST.Domain.Entities.Subscriptions;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class PlanConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Price).HasColumnType("numeric").IsRequired();
            // Varsayılan değerler C# Entity'de tanımlanmıştır.

            // PlanFeatures koleksiyonu bu sınıf içinde tanımlanabilir.
            builder.HasMany(p => p.Features)
                   .WithOne(pf => pf.Plan)
                   .HasForeignKey(pf => pf.PlanId)
                   .OnDelete(DeleteBehavior.Cascade); // Plan silinirse özellikler silinsin.
        }
    }
}