using System.Text.Json.Serialization;
using ST.Application.Interfaces.Messages;

namespace ST.Application.DTOs.Messages.EmailTemplates;

public class VerificationEmailTemplateDto : IMailgunTemplateData
{
    [JsonPropertyName("user_first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("email_verification_url")]
    public string VerificationUrl { get; set; }

    [JsonPropertyName("unsubscribe_url")]
    public string UnsubscribeUrl { get; set; }
}