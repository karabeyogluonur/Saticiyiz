using ST.Domain.Enums;

namespace ST.Application.DTOs.Billing
{
    public class BillingProfileDto
    {
        public int Id { get; set; }
        public bool IsPrimary { get; set; }
        public BillingAccountType BillingAccountType { get; set; }

        // --- Tüm Alanlar Eklendi ---
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string Address { get; set; } = default!;
        public int CityId { get; set; } // Eklendi
        public string CityName { get; set; } = default!;
        public int DistrictId { get; set; } // Eklendi
        public string DistrictName { get; set; } = default!;
        public string PostalCode { get; set; } = default!;

        public string? IndividualFirstName { get; set; }
        public string? IndividualLastName { get; set; }
        public string? IndividualIdentityNumber { get; set; } // Eklendi

        public string? CorporateCompanyName { get; set; }
        public string? CorporateTaxOffice { get; set; } // Eklendi
        public string? CorporateTaxNumber { get; set; } // Eklendi
    }
}