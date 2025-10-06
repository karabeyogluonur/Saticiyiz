using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Infrastructure.Identity;
using ST.Domain.Entities;

namespace ST.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // TenantId zorunluluğu ve boyutu
            builder.Property(u => u.TenantId)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.HasOne(u => u.Tenant)
                   .WithMany() // ApplicationTenant'taki koleksiyonu görmezden gelir
                   .HasForeignKey(u => u.TenantId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict); // <-- Bu satırın Düzgün SQL üretmesini ZORLUYORUZ.

            // TenantId üzerinde index
            builder.HasIndex(u => u.TenantId);
        }
    }
}