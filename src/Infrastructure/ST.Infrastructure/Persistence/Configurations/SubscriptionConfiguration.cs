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

            builder.HasOne(s => s.Plan)
                   .WithMany(p => p.Subscriptions)
                   .HasForeignKey(s => s.PlanId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.PlanId);
        }
    }
}
