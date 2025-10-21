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
using ST.Application.Interfaces.Identity;

namespace ST.Application.Features.Identity.Commands.EmailVerification;

public class SendEmailVerificationHandler : IRequestHandler<SendEmailVerificationCommand, Response<string>>
{
    private readonly IUserService _userService;
    private readonly IUrlHelperService _urlHelperService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SendEmailVerificationHandler(
        IUserService userService,
        IUrlHelperService urlHelperService,
        IBackgroundJobClient backgroundJobClient)
    {
        _urlHelperService = urlHelperService;
        _backgroundJobClient = backgroundJobClient;
        _userService = userService;
    }

    public async Task<Response<string>> Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser user = await _userService.GetUserByEmailAsync(request.Email, true);

        if (user == null)
            return Response<string>.Error("Bu e-posta ile kayıtlı kullanıcı bulunamadı.");

        if (user.EmailConfirmed)
            return Response<string>.Error("E-posta zaten doğrulanmış.");

        string identityToken = await _userService.GenerateEmailConfirmationTokenAsync(user);

        string emailVerificationUrl = await _urlHelperService.CreateEmailConfirmationUrlAsync(user.Email, identityToken);

        string unsubscribeUrl = await _urlHelperService.BuildUnsubscribeUrlAsync(user.Email);

        string emailTemplateJson = JsonSerializer.Serialize(new VerificationEmailTemplateDto
        {
            VerificationUrl = emailVerificationUrl,
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
