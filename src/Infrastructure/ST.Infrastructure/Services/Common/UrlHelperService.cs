using Microsoft.AspNetCore.Http;
using ST.Application.Constants;
using ST.Application.DTOs.Identity;
using ST.Application.Interfaces.Common;
using ST.Application.Interfaces.Security;
using System;

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

        public string BuildAbsoluteUrl(string relativePath, Dictionary<string, string>? queryParams = null)
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

            return url;
        }

        public string BuildUnsubscribeUrl(string token)
        {
            HttpRequest request = _httpContextAccessor.HttpContext?.Request;

            if (request == null)
                throw new InvalidOperationException("HttpContext bulunamadı.");

            string scheme = request.Scheme;
            string host = request.Host.Value;
            string baseUrl = $"{scheme}://{host}";

            string unsubscribeUrl = $"{baseUrl}/auth/unsubscribe?token={Uri.EscapeDataString(token)}";

            return unsubscribeUrl;
        }

        public string CreatePasswordResetUrl(string email, string identityToken)
        {
            var payload = new ResetPasswordPayloadDto { Email = email, IdentityToken = identityToken };
            string protectedData = _protectedDataService.Protect(payload, DataProtectionPurposes.PasswordReset);

            return BuildAbsoluteUrl("/Auth/ResetPassword",
                new Dictionary<string, string> { { "token", protectedData } }
            );
        }
    }
}
