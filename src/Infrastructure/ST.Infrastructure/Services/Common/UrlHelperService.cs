using Microsoft.AspNetCore.Http;
using ST.Application.Interfaces.Common;
using System;

namespace ST.Infrastructure.Services.Common
{

    public class UrlHelperService : IUrlHelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlHelperService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
    }
}
