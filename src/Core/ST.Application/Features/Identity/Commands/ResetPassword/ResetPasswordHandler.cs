using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Response<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProtectedDataService _protectedDataService;

        public ResetPasswordHandler(
            UserManager<ApplicationUser> userManager,
            IProtectedDataService protectedDataService)
        {
            _userManager = userManager;
            _protectedDataService = protectedDataService;
        }

        public async Task<Response<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Response<string>.Error("Token boş olamaz.");

            ResetPasswordPayloadDto protectedToken;
            try
            {
                protectedToken = _protectedDataService.Unprotect<ResetPasswordPayloadDto>(
                    request.Token,
                    DataProtectionPurposes.PasswordReset
                );
            }
            catch
            {
                return Response<string>.Error("Geçersiz veya süresi dolmuş token.");
            }

            ApplicationUser user = await _userManager.FindByEmailAsync(protectedToken.Email);

            if (user == null)
                return Response<string>.Error("Kullanıcı bulunamadı.");

            IdentityResult result = await _userManager.ResetPasswordAsync(
                user, protectedToken.IdentityToken, request.NewPassword);

            if (!result.Succeeded)
                return Response<string>.Error("Şifre sıfırlama işlemi başarısız oldu:", 
                    result.Errors.Select(e => e.Description));

            return Response<string>.Error("Şifreniz başarıyla güncellendi.");
        }
    }
}
