using Finbuckle.MultiTenant;
using Serilog.Context;
using System.Security.Claims;
using ST.Domain.Entities; // ApplicationTenant sınıfınızın olduğu yer

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
        // LogContext'e eklenecek bilgileri topla
        var tenant = context.GetMultiTenantContext<ApplicationTenant>()?.TenantInfo;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        // LogContext.PushProperty, using bloğu boyunca tüm loglara bu bilgileri ekler.
        // Bu yapı, asenkron ve paralel isteklerde bilgilerin karışmasını engeller.
        using (LogContext.PushProperty("TenantId", tenant?.Identifier ?? "none"))
        using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
        {
            await _next(context);
        }
    }
}