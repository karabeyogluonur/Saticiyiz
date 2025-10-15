using Microsoft.Extensions.Primitives;
using ST.Application.DTOs.Identity;
using ST.Domain.Interfaces;

namespace ST.Application.Interfaces.Security
{
    public interface IProtectedDataService
    {
        string Protect<T>(T data, string purpose);
        string Protect(string plainText, string purpose);

        T Unprotect<T>(string protectedData, string purpose);
        string Unprotect(string protectedData, string purpose);




        (ResetPasswordPayloadDto? Payload, string? ErrorMessage) UnprotectPasswordResetToken(string protectedToken);
        (EmailVerificationPayloadDto? Payload, string? ErrorMessage) UnprotectEmailVerificationToken(string protectedToken);
        (string? Email, string? ErrorMessage) UnprotectUnsubscribeToken(string protectedToken);
    }
}
