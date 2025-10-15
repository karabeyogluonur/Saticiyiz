using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities.Identity;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.Property(r => r.Description).HasMaxLength(999);
            builder.HasIndex(p => new { p.TenantId, p.NormalizedName }).IsUnique();

            var indexToRemove = builder.HasIndex(p => new { p.NormalizedName }).Metadata.Properties;
            builder.Metadata.RemoveIndex(indexToRemove);
        }
    }
}
