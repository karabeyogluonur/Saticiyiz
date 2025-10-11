using System.ComponentModel.DataAnnotations;

namespace ST.App.Features.Auth.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Display(Name = "Şifre")]
        public string Password { get; set; }

    }
}
