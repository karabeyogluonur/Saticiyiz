using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities;
using ST.Domain.Entities.Configurations;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Key).IsRequired().HasMaxLength(128);

            builder.HasIndex(s => new { s.Key, s.TenantId })
                   .IsUnique()
                   .HasFilter("\"TenantId\" IS NOT NULL");
        }
    }
}