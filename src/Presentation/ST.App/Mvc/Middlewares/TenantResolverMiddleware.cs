using ST.Application.Common.Constants;
using ST.Application.Interfaces.Tenancy;


namespace ST.App.Mvc.Middlewares
{
    public class TenantResolverMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolverMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentTenantStore currentTenantStore)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantIdClaim = context.User.FindFirst(c => c.Type == CustomClaims.TenantId);

                if (tenantIdClaim != null)
                {
                    if (int.TryParse(tenantIdClaim.Value, out var tenantId))
                    {
                        currentTenantStore.SetTenant(tenantId);
                    }
                }
            }
            await _next(context);
        }
    }
}
