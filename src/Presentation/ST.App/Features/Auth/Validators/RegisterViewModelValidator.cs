using FluentValidation;
using ST.App.Features.Auth.ViewModels;

namespace ST.App.Features.Auth.Validators
{
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator()
        {
            RuleFor(v => v.FirstName)
                .NotEmpty().WithMessage("Ad alanı zorunludur.")
                .MaximumLength(50).WithMessage("Ad alanı 50 karakterden uzun olamaz.");

            RuleFor(v => v.LastName)
                .NotEmpty().WithMessage("Soyad alanı zorunludur.")
                .MaximumLength(50).WithMessage("Soyad alanı 50 karakterden uzun olamaz.");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("E-posta adresi zorunludur.")
                .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.");

            RuleFor(v => v.PhoneNumber)
                .NotEmpty().WithMessage("Telefon numarası zorunludur.")
                .Matches(@"^(5(\d{9}))$").WithMessage("Lütfen geçerli bir telefon numarası giriniz (Örn: 5xxxxxxxxx).");

            RuleFor(v => v.Password)
                .NotEmpty().WithMessage("Şifre zorunludur.")
                .MinimumLength(8).WithMessage("Şifreniz en az 8 karakter olmalıdır.")
                .Matches("[A-Z]").WithMessage("Şifreniz en az bir büyük harf içermelidir.")
                .Matches("[a-z]").WithMessage("Şifreniz en az bir küçük harf içermelidir.")
                .Matches("[0-9]").WithMessage("Şifreniz en az bir rakam içermelidir.");

            RuleFor(v => v.ConfirmPassword)
                .NotEmpty().WithMessage("Şifre tekrar alanı zorunludur.")
                .Equal(v => v.Password).WithMessage("Girdiğiniz şifreler uyuşmuyor.");
        }
    }
}
