using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
        {
            builder.HasOne<ApplicationTenant>()
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
