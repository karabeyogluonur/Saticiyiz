using Microsoft.AspNetCore.Http;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST.Infrastructure.Services.Common
{
    public class UrlHelperService : IUrlHelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProtectedDataService _protectedDataService;

        public UrlHelperService(IHttpContextAccessor httpContextAccessor, IProtectedDataService protectedDataService)
        {
            _httpContextAccessor = httpContextAccessor;
            _protectedDataService = protectedDataService;
        }

        public async Task<string> BuildAbsoluteUrlAsync(string relativePath, Dictionary<string, string>? queryParams = null)
        {
            HttpRequest request = _httpContextAccessor.HttpContext?.Request;

            if (request == null)
                throw new InvalidOperationException("HttpContext bulunamadı.");

            string scheme = request.Scheme;
            string host = request.Host.Value;
            string baseUrl = $"{scheme}://{host}";

            string url = $"{baseUrl}{relativePath}";

            if (queryParams != null && queryParams.Count > 0)
            {
                string queryString = string.Join("&", queryParams.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"
                ));
                url = $"{url}?{queryString}";
            }

            return await Task.FromResult(url);
        }

        public async Task<string> BuildUnsubscribeUrlAsync(string email)
        {
            string unsubscribeProtectedData = _protectedDataService.Protect(email, DataProtectionPurposes.UnsubscribeNewsletter);

            string url = await BuildAbsoluteUrlAsync("/Auth/Unsubscribe",
                new Dictionary<string, string> { { "token", unsubscribeProtectedData } }
            );

            return await Task.FromResult(url);
        }

        public async Task<string> CreatePasswordResetUrlAsync(string email, string identityToken)
        {
            var payload = new ResetPasswordPayloadDto { Email = email, IdentityToken = identityToken };

            string protectedData = _protectedDataService.Protect(payload, DataProtectionPurposes.PasswordReset);

            string url = await BuildAbsoluteUrlAsync("/Auth/ResetPassword",
                new Dictionary<string, string> { { "token", protectedData } }
            );

            return url;
        }

        public async Task<string> CreateEmailConfirmationUrlAsync(string email, string identityToken)
        {
            var payload = new EmailVerificationPayloadDto(email, identityToken);

            string protectedData = _protectedDataService.Protect(payload, DataProtectionPurposes.EmailVerification);

            string url = await BuildAbsoluteUrlAsync("/Auth/VerifyEmail",
                new Dictionary<string, string> { { "token", protectedData } }
            );

            return url;
        }
    }
}
