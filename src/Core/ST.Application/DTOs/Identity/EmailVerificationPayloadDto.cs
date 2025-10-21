
using ST.Application.Interfaces.Messages;

namespace ST.Application.DTOs.Identity;

public class EmailVerificationPayloadDto : IMailgunTemplateData
{
    public EmailVerificationPayloadDto(string email, string identityToken)
    {
        Email = email;
        IdentityToken = identityToken;
    }
    public string Email { get; set; } = String.Empty;
    public string IdentityToken { get; set; } = String.Empty;
}
