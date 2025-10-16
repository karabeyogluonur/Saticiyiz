using Microsoft.AspNetCore.Mvc.Rendering;
using ST.Domain.Enums;

namespace ST.App.Features.Billing.ViewModels
{
    public class BillingProfileAddViewModel
    {
        public BillingAccountType BillingAccountType { get; set; }

        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string Address { get; set; } = default!;
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public string PostalCode { get; set; } = default!;

        public string? IndividualFirstName { get; set; }
        public string? IndividualLastName { get; set; }
        public string? IndividualIdentityNumber { get; set; }

        public string? CorporateCompanyName { get; set; }
        public string? CorporateTaxOffice { get; set; }
        public string? CorporateTaxNumber { get; set; }

        public IEnumerable<SelectListItem> CityList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DistrictList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BillingAccountTypeList { get; set; } = new List<SelectListItem>();
    }
}
