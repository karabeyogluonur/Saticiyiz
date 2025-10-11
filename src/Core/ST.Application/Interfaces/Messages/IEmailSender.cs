using ST.Application.DTOs.Messages;

namespace ST.Application.Interfaces.Messages;

public interface IEmailSender
{
    Task SendTemplateMailAsync(string to, string subject, string templateName, string templateDataJson);
}
