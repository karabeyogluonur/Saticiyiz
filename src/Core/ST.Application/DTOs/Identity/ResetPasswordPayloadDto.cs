
using ST.Application.Interfaces.Messages;

namespace ST.Application.DTOs.Identity;

public class ResetPasswordPayloadDto : IMailgunTemplateData
{
    public string Email { get; set; } = String.Empty;
    public string IdentityToken { get; set; } = String.Empty;
}
