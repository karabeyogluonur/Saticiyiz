using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Messages;
using ST.Application.Interfaces.Security;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;
using ST.Application.DTOs.Messages.EmailTemplates;

namespace ST.Application.Features.Identity.Commands.EmailVerification;

public class SendEmailVerificationHandler : IRequestHandler<SendEmailVerificationCommand, Response<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProtectedDataService _protectedDataService;
    private readonly IUrlHelperService _urlHelperService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SendEmailVerificationHandler(
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

    public async Task<Response<string>> Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Response<string>.Error("Bu e-posta ile kayıtlı kullanıcı bulunamadı.");

        if (user.EmailConfirmed)
            return Response<string>.Error("E-posta zaten doğrulanmış.");

        string identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        EmailVerificationPayloadDto payload = new EmailVerificationPayloadDto
        {
            Email = user.Email,
            IdentityToken = identityToken
        };

        // Token'ı koru / şifrele
        string verificationProtectedData = _protectedDataService.Protect(payload, DataProtectionPurposes.EmailVerification);

        // Doğrulama linki oluştur
        string verificationUrl = _urlHelperService.BuildAbsoluteUrl("/Auth/VerifyEmail",
            new Dictionary<string, string> { { "token", verificationProtectedData } }
        );

        string unsubscribeProtectedData = _protectedDataService.Protect(user.Email, DataProtectionPurposes.UnsubscribeNewsletter);

        string unsubscribeUrl = _urlHelperService.BuildUnsubscribeUrl(unsubscribeProtectedData);

        string emailTemplateJson = JsonSerializer.Serialize(new VerificationEmailTemplateDto
        {
            VerificationUrl = verificationUrl,
            FirstName = user.FirstName,
            UnsubscribeUrl = unsubscribeUrl
        });

        _backgroundJobClient.Enqueue<IEmailSender>(sender =>
            sender.SendTemplateMailAsync(
                user.Email,
                $"Aramıza hoş geldin {user.FirstName}! Şimdi e-posta adresini doğrula 🚀",
                "saticiyiz-app-email-verification",
                emailTemplateJson
            )
        );

        return Response<string>.Success("E-posta doğrulama linki gönderildi.");
    }
}
