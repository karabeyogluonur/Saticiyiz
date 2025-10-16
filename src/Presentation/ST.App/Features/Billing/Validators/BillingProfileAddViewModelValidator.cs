using FluentValidation;
using ST.App.Features.Billing.ViewModels;
using ST.Domain.Enums;

namespace ST.App.Features.Billing.Validators
{
    public class BillingProfileAddViewModelValidator : AbstractValidator<BillingProfileAddViewModel>
    {
        public BillingProfileAddViewModelValidator()
        {
            RuleFor(x => x.BillingAccountType)
                .IsInEnum()
                .WithMessage("Lütfen geçerli bir fatura tipi seçiniz.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi zorunludur.")
                .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.")
                .MaximumLength(255).WithMessage("E-posta adresi en fazla 255 karakter olabilir.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Adres zorunludur.")
                .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("Şehir seçimi zorunludur.");

            RuleFor(x => x.DistrictId)
                .GreaterThan(0).WithMessage("İlçe seçimi zorunludur.");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("Posta Kodu zorunludur.")
                .Matches("^[0-9]{5}$").WithMessage("Posta kodu 5 haneli bir sayı olmalıdır.");

            // --- Bireysel Alanlar İçin Koşullu Validasyon ---
            When(x => x.BillingAccountType == BillingAccountType.Individual, () =>
            {
                RuleFor(x => x.IndividualFirstName)
                    .NotEmpty().WithMessage("Ad alanı zorunludur.")
                    .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

                RuleFor(x => x.IndividualLastName)
                    .NotEmpty().WithMessage("Soyad alanı zorunludur.")
                    .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");

                RuleFor(x => x.IndividualIdentityNumber)
                    .NotEmpty().WithMessage("TC Kimlik Numarası zorunludur.")
                    .Length(11).WithMessage("TC Kimlik Numarası 11 haneli olmalıdır.")
                    .Matches("^[0-9]{11}$").WithMessage("TC Kimlik Numarası sadece rakamlardan oluşmalıdır.");
            });

            // --- Kurumsal Alanlar İçin Koşullu Validasyon ---
            When(x => x.BillingAccountType == BillingAccountType.Corporate, () =>
            {
                RuleFor(x => x.CorporateCompanyName)
                    .NotEmpty().WithMessage("Şirket Adı zorunludur.")
                    .MaximumLength(255).WithMessage("Şirket adı en fazla 255 karakter olabilir.");

                RuleFor(x => x.CorporateTaxOffice)
                    .NotEmpty().WithMessage("Vergi Dairesi zorunludur.")
                    .MaximumLength(150).WithMessage("Vergi dairesi en fazla 150 karakter olabilir.");

                RuleFor(x => x.CorporateTaxNumber)
                    .NotEmpty().WithMessage("Vergi Numarası zorunludur.")
                    .Matches("^[0-9]{10,11}$").WithMessage("Vergi Numarası 10 veya 11 haneli olmalıdır.");
            });
        }
    }
}