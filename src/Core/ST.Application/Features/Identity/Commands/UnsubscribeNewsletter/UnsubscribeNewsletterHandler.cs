using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Features.Identity.Commands.UnsubscribeNewsletter;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.UnsubscribeNewsletter
{
    public class UnsubscribeNewsletterHandler : IRequestHandler<UnsubscribeNewsletterCommand, Response<string>>
    {
        private readonly IUserService _userService;
        private readonly IProtectedDataService _protectedDataService;
        private readonly IUserContext _userContext;

        public UnsubscribeNewsletterHandler(
            IUserService userService,
            IProtectedDataService protectedDataService,
            IUserContext userContext)
        {
            _userService = userService;
            _protectedDataService = protectedDataService;
            _userContext = userContext;
        }

        public async Task<Response<string>> Handle(UnsubscribeNewsletterCommand request, CancellationToken cancellationToken)
        {
            var (userEmail, tokenError) = _protectedDataService.UnprotectUnsubscribeToken(request.Token);

            if (userEmail == null)
                return Response<string>.Error(tokenError!);

            ApplicationUser user = await _userService.GetUserByEmailAsync(userEmail);

            if (user == null)
                return Response<string>.Error("Kullanıcı bulunamadı. Lütfen destek ekibi ile iletişime geçiniz!");

            // 3. Yetkilendirme kontrolü (Handler'a özgü mantık)
            string currentUserEmail = _userContext.EmailOrUsername;

            if (!string.IsNullOrEmpty(currentUserEmail) && !string.Equals(currentUserEmail, userEmail, StringComparison.OrdinalIgnoreCase))
                return Response<string>.Error("Bu token sizin hesabınıza ait değil.");

            if (!user.IsSubscribedToNewsletter)
                return Response<string>.Error("Mevcut bir bülten aboneliğiniz bulunmamaktadır.");

            IdentityResult updateResult = await _userService.UnsubscribeFromNewsletterAsync(user);

            if (!updateResult.Succeeded)
            {
                IEnumerable<string> errors = updateResult.Errors.Select(e => e.Description);
                return Response<string>.Error("Bülten aboneliği iptali başarısız.", errors);
            }

            return Response<string>.Success("Bülten aboneliğiniz başarıyla iptal edildi.");
        }
    }
}
