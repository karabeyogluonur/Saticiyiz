using Microsoft.AspNetCore.Http;
using System;

namespace ST.Application.Interfaces.Common
{
    public interface IUrlHelperService
    {
        Task<string> BuildAbsoluteUrlAsync(string relativePath, Dictionary<string, string>? queryParams = null);
        Task<string> BuildUnsubscribeUrlAsync(string token);
        Task<string> CreatePasswordResetUrlAsync(string email, string identityToken);
        Task<string> CreateEmailConfirmationUrlAsync(string email, string identityToken);
    }


}
