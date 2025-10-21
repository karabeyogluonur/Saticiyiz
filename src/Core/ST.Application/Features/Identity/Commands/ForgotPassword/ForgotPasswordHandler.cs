using System.Text.Json;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.DTOs.Messages.EmailTemplates;
using ST.Application.Features.Identity.Commands.ForgotPassword;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Messages;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;
using ST.Domain.Identity;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Response<string>>
{
    private readonly IUserService _userService;
    private readonly IUrlHelperService _urlHelperService;
    IBackgroundJobClient _backgroundJobClient;

    public ForgotPasswordCommandHandler(
        IUserService userService,
        IUrlHelperService urlHelperService,
        IBackgroundJobClient backgroundJobClient)
    {
        _userService = userService;
        _urlHelperService = urlHelperService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<Response<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser user = await _userService.GetUserByEmailAsync(request.Email, true);

        if (user == null)
            return Response<string>.Error("Bu e-posta adresine kayıtlı kullanıcı bulunamadı!");

        string identityToken = await _userService.GeneratePasswordResetTokenAsync(user);

        string resetUrl = await _urlHelperService.CreatePasswordResetUrlAsync(user.Email, identityToken);

        string forgotPasswordEmailJson = JsonSerializer.Serialize(new ForgotPasswordEmailTemplateDto(resetUrl));

        _backgroundJobClient.Enqueue<IEmailSender>(sender => sender.SendTemplateMailAsync(user.Email, "Şifremi Unuttum - Satıcıyız", "saticiyiz-app-forgot-password", forgotPasswordEmailJson));

        return Response<string>.Success("Şifre sıfırlama linki e-posta adresinize gönderildi.");
    }
}