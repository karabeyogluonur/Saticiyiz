using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Infrastructure.Identity;
using ST.Domain.Entities;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.Property(r => r.Description).HasMaxLength(256);

            builder.HasIndex(r => new { r.TenantId, r.NormalizedName })
                   .HasName("IX_Role_Tenant_Name")
                   .IsUnique();

            builder.HasOne<ApplicationTenant>()
                   .WithMany()
                   .HasForeignKey(r => r.TenantId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}