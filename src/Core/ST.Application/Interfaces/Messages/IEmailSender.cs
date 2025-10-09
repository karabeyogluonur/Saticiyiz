using ST.Application.DTOs.Messages;

namespace ST.Application.Interfaces.Messages;

/// <summary>
/// Uygulamanın e-posta göndermek için kullanacağı ana servis.
/// Ayarlara bakarak doğru IEmailProvider'ı seçip görevi ona devreder.
/// </summary>
public interface IEmailSender
{
    Task SendTemplateMailAsync(string to, string subject, string templateName, string templateDataJson);
}