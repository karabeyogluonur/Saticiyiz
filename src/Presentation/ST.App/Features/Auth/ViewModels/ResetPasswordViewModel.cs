using System.ComponentModel.DataAnnotations;

namespace ST.App.Features.Auth.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}