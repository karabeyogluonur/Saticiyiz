using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Infrastructure.Identity;
using ST.Domain.Entities; // ApplicationTenant için

namespace ST.Infrastructure.Persistence.Configurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            // Description alanı kısıtlaması
            builder.Property(r => r.Description).HasMaxLength(256);

            // KRİTİK: Multi-Tenant Role Konfigürasyonu
            // Identity'deki Rol adının, aynı kiracı içinde benzersiz olması gerekir.
            builder.HasIndex(r => new { r.TenantId, r.NormalizedName })
                   .HasName("IX_Role_Tenant_Name") // İsimlendir
                   .IsUnique();

            // 1. İlişki: ApplicationRole -> ApplicationTenant
            // Rolün bir Kiracıya ait olduğunu belirtir.
            builder.HasOne<ApplicationTenant>()
                   .WithMany() // ApplicationTenant'ta Role koleksiyonu tutmuyoruz.
                   .HasForeignKey(r => r.TenantId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); // Kiracı silinirse tüm roller silinsin (Güvenli varsayım).
        }
    }
}