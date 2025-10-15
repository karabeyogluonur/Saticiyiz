using Microsoft.AspNetCore.DataProtection;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Interfaces.Security;
using System.Text.Json;
using System.Threading.Tasks;

namespace ST.Infrastructure.Services.Security
{
    public class ProtectedDataService : IProtectedDataService
    {
        private readonly IDataProtectionProvider _provider;

        public ProtectedDataService(IDataProtectionProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        #region Protect

        public string Protect<T>(T data, string purpose)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Purpose boş olamaz", nameof(purpose));

            string json = JsonSerializer.Serialize(data);
            IDataProtector protector = _provider.CreateProtector(purpose);
            return protector.Protect(json);
        }

        public string Protect(string plainText, string purpose)
        {
            if (string.IsNullOrWhiteSpace(plainText)) throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Purpose boş olamaz", nameof(purpose));

            IDataProtector protector = _provider.CreateProtector(purpose);
            return protector.Protect(plainText);
        }

        #endregion

        #region Unprotect

        public T Unprotect<T>(string protectedData, string purpose)
        {
            if (string.IsNullOrWhiteSpace(protectedData)) throw new ArgumentNullException(nameof(protectedData));
            if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Purpose boş olamaz", nameof(purpose));

            IDataProtector protector = _provider.CreateProtector(purpose);
            string json = protector.Unprotect(protectedData);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        public string Unprotect(string protectedData, string purpose)
        {
            if (string.IsNullOrWhiteSpace(protectedData)) throw new ArgumentNullException(nameof(protectedData));
            if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Purpose boş olamaz", nameof(purpose));

            IDataProtector protector = _provider.CreateProtector(purpose);
            return protector.Unprotect(protectedData);
        }

        #endregion

        public (ResetPasswordPayloadDto? Payload, string? ErrorMessage) UnprotectPasswordResetToken(string protectedToken)
        {
            try
            {
                var payload = Unprotect<ResetPasswordPayloadDto>(protectedToken, DataProtectionPurposes.PasswordReset);
                return (payload, null);
            }
            catch
            {
                return (null, "Geçersiz veya süresi dolmuş token.");
            }
        }
        public (EmailVerificationPayloadDto? Payload, string? ErrorMessage) UnprotectEmailVerificationToken(string protectedToken)
        {
            try
            {
                var payload = Unprotect<EmailVerificationPayloadDto>(protectedToken, DataProtectionPurposes.EmailVerification);
                return (payload, null);
            }
            catch
            {
                return (null, "Geçersiz veya süresi dolmuş token.");
            }
        }
        public (string? Email, string? ErrorMessage) UnprotectUnsubscribeToken(string protectedToken)
        {
            try
            {
                string email = Unprotect(protectedToken, DataProtectionPurposes.UnsubscribeNewsletter);
                return (email, null);
            }
            catch
            {
                return (null, "Geçersiz veya süresi dolmuş token.");
            }
        }
    }
}
