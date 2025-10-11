using Serilog.Context;
using ST.Application.Interfaces.Tenancy;
using System.Security.Claims;

namespace ST.App.Mvc.Middlewares;

public class LogContextMiddleware
{
    private readonly RequestDelegate _next;

    public LogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context, ICurrentTenantStore currentTenantStore)
    {
        var tenantId = currentTenantStore.Id?.ToString() ?? "none";

        string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("UserId", userId))
        {
            await _next(context);
        }
    }
}
