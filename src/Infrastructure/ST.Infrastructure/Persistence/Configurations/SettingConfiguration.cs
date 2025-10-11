
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities.Configurations;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Key).IsRequired().HasMaxLength(128);
            builder.HasIndex(s => s.Key).IsUnique();
        }
    }
}
