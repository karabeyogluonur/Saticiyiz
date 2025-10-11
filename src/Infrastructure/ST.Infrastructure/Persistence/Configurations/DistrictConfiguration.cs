using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities.Lookup;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class DistrictConfiguration : IEntityTypeConfiguration<District>
    {
        public void Configure(EntityTypeBuilder<District> builder)
        {
            builder.ToTable("Districts");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(d => new { d.CityId, d.Name })
                   .IsUnique();

            builder.HasOne(d => d.City)
                   .WithMany(c => c.Districts)
                   .HasForeignKey(d => d.CityId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
