namespace ST.Application.Interfaces.Messages;

public interface IEmailProvider
{
    Task SendTemplateMailAsync(string to, string subject, string templateName, string templateDataJson);
}