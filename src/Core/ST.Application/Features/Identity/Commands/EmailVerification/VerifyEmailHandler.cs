using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.EmailVerification
{
    public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, Response<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProtectedDataService _protectedDataService;
        private readonly IUserContext _userContext;

        public VerifyEmailHandler(
            UserManager<ApplicationUser> userManager,
            IProtectedDataService protectedDataService,
            IUserContext userContext)
        {
            _userManager = userManager;
            _protectedDataService = protectedDataService;
            _userContext = userContext;
        }

        public async Task<Response<string>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            EmailVerificationPayloadDto payload;
            try
            {
                payload = _protectedDataService.Unprotect<EmailVerificationPayloadDto>(
                    request.Token,
                    DataProtectionPurposes.EmailVerification
                );
            }
            catch
            {
                return Response<string>.Error("Geçersiz veya süresi dolmuş token.");
            }

            ApplicationUser user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
                return Response<string>.Error("Kullanıcı bulunamadı.");

            if (user.EmailConfirmed)
                return Response<string>.Error("E-posta zaten doğrulanmış.");

            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(
                user, payload.IdentityToken);

            if (!identityResult.Succeeded)
            {
                IEnumerable<string> errors = identityResult.Errors.Select(e => e.Description);
                return Response<string>.Error("E-posta doğrulama başarısız.", errors);
            }

            return Response<string>.Success("E-posta başarıyla doğrulandı.");
        }
    }
}
