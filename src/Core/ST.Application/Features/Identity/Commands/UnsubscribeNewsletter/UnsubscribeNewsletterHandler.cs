using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Features.Identity.Commands.UnsubscribeNewsletter;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.UnsubscribeNewsletter
{
    public class UnsubscribeNewsletterHandler : IRequestHandler<UnsubscribeNewsletterCommand, Response<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProtectedDataService _protectedDataService;
        private readonly IUserContext _userContext;

        public UnsubscribeNewsletterHandler(
            UserManager<ApplicationUser> userManager,
            IProtectedDataService protectedDataService,
            IUserContext userContext)
        {
            _userManager = userManager;
            _protectedDataService = protectedDataService;
            _userContext = userContext;
        }

        public async Task<Response<string>> Handle(UnsubscribeNewsletterCommand request, CancellationToken cancellationToken)
        {
            string userEmail;
            try
            {
                userEmail = _protectedDataService.Unprotect(request.Token, DataProtectionPurposes.UnsubscribeNewsletter);
            }
            catch
            {
                return Response<string>.Error("Geçersiz veya süresi dolmuş token.");
            }

            ApplicationUser user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
                return Response<string>.Error("Kullanıcı bulunamadı.");

            string currentUserEmail = _userContext.Username;

            if (!string.IsNullOrEmpty(currentUserEmail) && !string.Equals(currentUserEmail, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Response<string>.Error("Bu token sizin hesabınıza ait değil.");
            }

            if (!user.IsSubscribedToNewsletter)
                return Response<string>.Error("Mevcut bir bülten aboneliğiniz bulunmamaktadır.");

            user.IsSubscribedToNewsletter = false;
            user.NewsletterSubscribedAt = null;

            IdentityResult updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                IEnumerable<string> errors = updateResult.Errors.Select(e => e.Description);
                return Response<string>.Error("Bülten aboneliği iptali başarısız.", errors);
            }

            return Response<string>.Success("Bülten aboneliğiniz başarıyla iptal edildi.");
        }
    }
}
