using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ST.Domain.Entities.Billing;

namespace ST.Infrastructure.Persistence.Configurations
{
       public class BillingProfileConfiguration : IEntityTypeConfiguration<BillingProfile>
       {
              public void Configure(EntityTypeBuilder<BillingProfile> builder)
              {
                     builder.ToTable("BillingProfiles");

                     builder.HasKey(x => x.Id);


                     builder.Property(x => x.BillingAccountType)
                            .IsRequired();

                     builder.Property(x => x.Email)
                            .IsRequired()
                            .HasMaxLength(200);

                     builder.Property(x => x.PhoneNumber)
                            .HasMaxLength(20);

                     builder.Property(x => x.Address)
                            .IsRequired()
                            .HasMaxLength(500);

                     builder.HasOne(x => x.City)
                            .WithMany()
                            .HasForeignKey(x => x.CityId)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.HasOne(x => x.District)
                            .WithMany()
                            .HasForeignKey(x => x.DistrictId)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.Property(x => x.PostalCode)
                            .IsRequired()
                            .HasMaxLength(10);

                     builder.Property(x => x.IndividualFirstName).HasMaxLength(100);
                     builder.Property(x => x.IndividualLastName).HasMaxLength(100);
                     builder.Property(x => x.IndividualIdentityNumber).HasMaxLength(20);

                     builder.Property(x => x.CorporateCompanyName).HasMaxLength(200);
                     builder.Property(x => x.CorporateTaxOffice).HasMaxLength(100);
                     builder.Property(x => x.CorporateTaxNumber).HasMaxLength(50);

                     builder.Property(x => x.CreatedBy)
                            .IsRequired()
                            .HasMaxLength(100);

                     builder.Property(x => x.CreatedDate)
                            .IsRequired();

                     builder.Property(x => x.LastModifiedBy)
                            .HasMaxLength(100);

                     builder.Property(x => x.LastModifiedDate);

                     builder.Property(x => x.IsDeleted)
                            .HasDefaultValue(false);

                     builder.Property(x => x.DeletedBy)
                            .HasMaxLength(100);

                     builder.Property(x => x.DeletedDate);
              }
       }
}
