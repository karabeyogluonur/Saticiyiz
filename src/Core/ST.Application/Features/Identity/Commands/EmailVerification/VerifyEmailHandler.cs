using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Features.Identity.Commands.EmailVerification;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.EmailVerification;

public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, Response<string>>
{
    private readonly IUserService _userService;
    private readonly IProtectedDataService _protectedDataService;

    public VerifyEmailHandler(
        IUserService userService,
        IProtectedDataService protectedDataService)
    {
        _userService = userService;
        _protectedDataService = protectedDataService;
    }

    public async Task<Response<string>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var (payload, tokenError) = _protectedDataService.UnprotectEmailVerificationToken(request.Token);

        if (payload == null)
            return Response<string>.Error(tokenError!);

        ApplicationUser user = await _userService.GetUserByEmailAsync(payload.Email);

        if (user == null)
            return Response<string>.Error("Kullanıcı bulunamadı. Doğrulama e-postasını tekrar göndererek tekrar deneyiniz!");

        if (user.EmailConfirmed)
        {
            return Response<string>.Error("E-posta zaten doğrulanmış.");
        }

        IdentityResult identityResult = await _userService.ConfirmEmailAsync(user, payload.IdentityToken);

        if (!identityResult.Succeeded)
        {
            IEnumerable<string> errors = identityResult.Errors.Select(e => e.Description);
            return Response<string>.Error("E-posta doğrulama başarısız.", errors);
        }

        return Response<string>.Success("E-posta başarıyla doğrulandı.");
    }
}