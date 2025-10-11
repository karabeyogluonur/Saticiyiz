using System.Security.Claims;
using ST.Application.Common.Constants;

namespace ST.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        string? userId = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out int parsedUserId) ? parsedUserId : throw new ApplicationException();
    }

    public static string GetUsername(this ClaimsPrincipal? claimsPrincipal)
    {
        string? username = claimsPrincipal?.FindFirstValue(ClaimTypes.Name);
        return username;
    }
}
