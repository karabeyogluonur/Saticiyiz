using System.Net;
using System.Text.Json;
using ST.Application.Exceptions;

namespace ST.App.Mvc.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred. Message: {Message}", ex.Message);

            HttpResponse response = context.Response;
            response.ContentType = "application/json";

            switch (ex)
            {
                case ValidationException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await response.WriteAsync(JsonSerializer.Serialize(new { errors = e.Errors }));
                    return;

                case NotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            object errorResponse = new
            {
                message = ex.Message,
                stackTrace = _env.IsDevelopment() ? ex.StackTrace : null
            };

            string result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }
}
