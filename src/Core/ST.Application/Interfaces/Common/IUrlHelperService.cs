using Microsoft.AspNetCore.Http;
using System;

namespace ST.Application.Interfaces.Common
{
    public interface IUrlHelperService
    {
        string BuildAbsoluteUrl(string relativePath, Dictionary<string, string>? queryParams = null);
        string BuildUnsubscribeUrl(string token);
    }


}
