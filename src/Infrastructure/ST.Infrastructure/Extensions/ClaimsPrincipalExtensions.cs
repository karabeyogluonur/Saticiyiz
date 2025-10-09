using System.Security.Claims;

namespace ST.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        string? userId = claimsPrincipal?.FindFirstValue("UserId");
        return int.TryParse(userId, out int parsedUserId) ? parsedUserId : throw new ApplicationException();
    }

    public static string GetUsername(this ClaimsPrincipal? claimsPrincipal)
    {
        string? username = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return username;
    }
}