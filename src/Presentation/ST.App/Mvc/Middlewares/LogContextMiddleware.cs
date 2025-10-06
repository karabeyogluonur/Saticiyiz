using Finbuckle.MultiTenant;
using Serilog.Context;
using System.Security.Claims;
using ST.Domain.Entities;

namespace ST.App.Mvc.Middlewares;

public class LogContextMiddleware
{
    private readonly RequestDelegate _next;

    public LogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenant = context.GetMultiTenantContext<ApplicationTenant>()?.TenantInfo;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        using (LogContext.PushProperty("TenantId", tenant?.Identifier ?? "none"))
        using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
        {
            await _next(context);
        }
    }
}