using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Text.Json;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.DTOs.Messages.EmailTemplates;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Messages;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Features.Identity.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Response<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProtectedDataService _protectedDataService;
    private readonly IUrlHelperService _urlHelperService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ForgotPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IProtectedDataService protectedDataService,
        IUrlHelperService urlHelperService,
        IBackgroundJobClient backgroundJobClient)
    {
        _userManager = userManager;
        _protectedDataService = protectedDataService;
        _urlHelperService = urlHelperService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<Response<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Response<string>.Error("Bu e-posta ile kayıtlı kullanıcı bulunamadı.");

        string identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        ResetPasswordPayloadDto payload = new ResetPasswordPayloadDto { Email = user.Email, IdentityToken = identityToken };

        string protectedData = _protectedDataService.Protect(payload, DataProtectionPurposes.PasswordReset);

        string resetUrl = _urlHelperService.BuildAbsoluteUrl("/Auth/ResetPassword",
            new Dictionary<string, string> { { "token", protectedData } }
        );

        string forgotPasswordEmailTemplateDtoJson = JsonSerializer.Serialize(
            new ForgotPasswordEmailTemplateDto(resetUrl));

        _backgroundJobClient.Enqueue<IEmailSender>(sender => 
            sender.SendTemplateMailAsync(user.Email, "Şifremi Unuttum - Satıcıyız", 
                "saticiyiz-app-forgot-password", forgotPasswordEmailTemplateDtoJson));

        return Response<string>.Success("Şifre sıfırlama linki e-posta adresinize gönderildi.");
    }
}
