using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class ApplicationTenantConfiguration : IEntityTypeConfiguration<ApplicationTenant>
    {
        public void Configure(EntityTypeBuilder<ApplicationTenant> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Identifier).IsRequired().HasMaxLength(64);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(256);

            builder.HasIndex(t => t.Identifier).IsUnique();
        }
    }
}