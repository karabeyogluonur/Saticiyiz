using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ST.Application.DTOs.Messages;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Messages;
using ST.Application.Settings;
using ST.Domain.Enums;
using ST.Infrastructure.Services.Email;


namespace ST.Infrastructure.Services.Messages;

public class DefaultEmailSender : IEmailSender
{
    private readonly ISettingService _settingService;
    private readonly IServiceProvider _serviceProvider;

    public DefaultEmailSender(ISettingService settingService, IServiceProvider serviceProvider)
    {
        _settingService = settingService;
        _serviceProvider = serviceProvider;
    }

    public async Task SendTemplateMailAsync(string to, string subject, string templateName, string templateDataJson)
    {
        EmailSetting emailSetting = await _settingService.GetGlobalSettingsAsync<EmailSetting>();

        IEmailProvider provider = emailSetting.Provider switch
        {
            EmailProvider.Mailgun => _serviceProvider.GetRequiredService<MailgunProvider>(),
            _ => throw new NotSupportedException($"E-posta sağlayıcısı desteklenmiyor.")
        };

        await provider.SendTemplateMailAsync(to, subject, templateName, templateDataJson);
    }
}