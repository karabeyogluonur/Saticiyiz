using Microsoft.AspNetCore.Http;
using ST.Application.Common.Constants;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ST.App.Mvc.Middlewares
{
    public class SetupMiddleware
    {
        private readonly RequestDelegate _next;

        public SetupMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                Claim setupCompleteClaim = context.User.FindFirst(CustomClaims.IsSetupComplete);

                if (setupCompleteClaim != null && setupCompleteClaim.Value == bool.FalseString)
                {
                    PathString path = context.Request.Path;
                    if (!path.StartsWithSegments("/Setup") && !path.StartsWithSegments("/Auth/Logout"))
                    {
                        context.Response.Redirect("/Setup");
                        return;
                    }
                }
            }
            await _next(context);
        }
    }
}