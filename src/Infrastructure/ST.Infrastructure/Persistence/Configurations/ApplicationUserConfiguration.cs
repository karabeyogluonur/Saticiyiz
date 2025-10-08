using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities.Identity;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.TenantId)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.HasOne(u => u.Tenant)
                   .WithMany()
                   .HasForeignKey(u => u.TenantId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(u => u.TenantId);
        }
    }
}