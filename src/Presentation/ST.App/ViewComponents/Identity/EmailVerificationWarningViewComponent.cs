using Microsoft.AspNetCore.Mvc;
using ST.Application.Common.Constants;
using System.Security.Claims;

namespace ST.App.ViewComponents.Identity
{
    public class EmailVerificationWarningViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            if (User is not ClaimsPrincipal claimsPrincipal)
            {
                return Content(string.Empty);
            }

            var emailConfirmedClaim = claimsPrincipal.FindFirst(c => c.Type == CustomClaims.EmailVerification);

            if (emailConfirmedClaim == null ||
                bool.TryParse(emailConfirmedClaim.Value, out var isConfirmed) && isConfirmed)
            {
                return Content(string.Empty);
            }

            return View();
        }
    }
}