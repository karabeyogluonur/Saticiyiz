using System.Text.Json.Serialization;
using ST.Application.Interfaces.Messages;

namespace ST.Application.DTOs.Messages.EmailTemplates;

public class ForgotPasswordEmailTemplateDto : IMailgunTemplateData
{
    public ForgotPasswordEmailTemplateDto(string resetUrl)
    {
        ResetUrl = resetUrl;
    }

    [JsonPropertyName("reset_url")]
    public string ResetUrl { get; set; }
}
