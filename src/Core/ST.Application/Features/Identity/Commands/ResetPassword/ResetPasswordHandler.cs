using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Response<string>>
{
    private readonly IUserService _userService;
    private readonly IProtectedDataService _protectedDataService;

    public ResetPasswordHandler(
        IUserService userService,
        IProtectedDataService protectedDataService)
    {
        _userService = userService;
        _protectedDataService = protectedDataService;
    }

    public async Task<Response<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var (payload, tokenError) = _protectedDataService.UnprotectPasswordResetToken(request.Token);

        if (payload == null)
            return Response<string>.Error(tokenError!);

        ApplicationUser user = await _userService.GetUserByEmailAsync(payload.Email, false);

        if (user == null)
            return Response<string>.Error("Kullanıcı bulunamadı. Lütfen destek ekibi ile iletişime geçiniz!"!);

        IdentityResult result = await _userService.ResetPasswordAsync(user, payload.IdentityToken, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Response<string>.Error("Şifre sıfırlama işlemi başarısız oldu:", errors);
        }

        return Response<string>.Success("Şifreniz başarıyla güncellendi.");
    }
}