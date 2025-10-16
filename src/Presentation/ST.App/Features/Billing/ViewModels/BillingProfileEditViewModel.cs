using Microsoft.AspNetCore.Mvc.Rendering;
using ST.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ST.App.Features.Billing.ViewModels
{
    public class BillingProfileEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Fatura Türü")]
        public BillingAccountType BillingAccountType { get; set; }

        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; } = default!;

        [Display(Name = "Telefon Numarası")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Açık Adres")]
        public string Address { get; set; } = default!;

        [Display(Name = "İl")]
        public int CityId { get; set; }

        [Display(Name = "İlçe")]
        public int DistrictId { get; set; }

        [Display(Name = "Posta Kodu")]
        public string PostalCode { get; set; } = default!;

        [Display(Name = "Ad")]
        public string? IndividualFirstName { get; set; }

        [Display(Name = "Soyad")]
        public string? IndividualLastName { get; set; }

        [Display(Name = "T.C. Kimlik Numarası")]
        public string? IndividualIdentityNumber { get; set; }

        [Display(Name = "Şirket Unvanı")]
        public string? CorporateCompanyName { get; set; }

        [Display(Name = "Vergi Dairesi")]
        public string? CorporateTaxOffice { get; set; }

        [Display(Name = "Vergi Numarası")]
        public string? CorporateTaxNumber { get; set; }

        public IEnumerable<SelectListItem> CityList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DistrictList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BillingAccountTypeList { get; set; } = new List<SelectListItem>();
    }
}