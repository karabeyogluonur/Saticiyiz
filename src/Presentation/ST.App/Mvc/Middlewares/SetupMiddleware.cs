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
            if (context.User.Identity?.IsAuthenticated != true || context.Request.Path.StartsWithSegments("/Setup"))
            {
                await _next(context);
                return;
            }

            var isSetupCompleteClaim = context.User.FindFirst(c => c.Type == CustomClaims.IsSetupCompleted);

            if (isSetupCompleteClaim == null || !bool.Parse(isSetupCompleteClaim.Value))
            {
                context.Response.Redirect("/Setup");
                return;
            }

            await _next(context);
        }
    }
}