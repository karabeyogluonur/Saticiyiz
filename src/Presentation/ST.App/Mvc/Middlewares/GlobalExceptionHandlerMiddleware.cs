using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ST.Application.Exceptions;

namespace ST.App.Mvc.Middlewares;

/// <summary>
/// Global exception handling middleware.
/// - Provides consistent JSON error responses (ProblemDetails-style)
/// - Avoids leaking sensitive details in Production
/// - Adds contextual logging (TraceId, Path, Method, User)
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Guard: if context is null something is seriously wrong — throw early.
        if (context is null) throw new ArgumentNullException(nameof(context));

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Capture trace identifier for correlation between logs and clients.
            string traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            // Create a log scope to include contextual information for the logger.
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["TraceId"] = traceId,
                ["RequestPath"] = context.Request.Path.Value,
                ["RequestMethod"] = context.Request.Method,
                ["User"] = context.User?.Identity?.Name
            }))
            {
                _logger.LogError(ex, "An unhandled exception has occurred. TraceId: {TraceId}", traceId);

                // Ensure we haven't already started writing the response.
                if (context.Response.HasStarted)
                {
                    // If response already started, we can't modify it. Log and rethrow.
                    _logger.LogWarning("The response has already started, the exception middleware will not be able to write the response body. TraceId: {TraceId}", traceId);
                    throw;
                }

                // Build a standard ProblemDetails-like object for structured responses.
                var problem = CreateProblemDetailsForException(ex, traceId);

                // Set response metadata (no cache, correct content type).
                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.Headers["Cache-Control"] = "no-store"; // Prevent caching error pages

                // Choose correct status code
                context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;

                // Write response
                var result = JsonSerializer.Serialize(problem, _jsonOptions);
                await context.Response.WriteAsync(result);
            }
        }
    }

    /// <summary>
    /// Maps exceptions to a ProblemDetails-like structure.
    /// - In Development returns stack trace for easier debugging.
    /// - In Production hides sensitive details and returns user-friendly Turkish messages.
    /// </summary>
    private ProblemDetails CreateProblemDetailsForException(Exception ex, string traceId)
    {
        // Default values (Internal Server Error)
        int status = (int)HttpStatusCode.InternalServerError;
        string title = "Sunucu hatası"; // Short title for problem
        string detail = _env.IsDevelopment() ? ex.Message : $"Sunucu tarafında beklenmeyen bir hata oluştu. Lütfen tekrar deneyin. (Hata Kodu: {traceId})";
        var extensions = new Dictionary<string, object?>
        {
            ["traceId"] = traceId,
            ["env"] = _env.EnvironmentName
        };

        switch (ex)
        {
            case ValidationException vex:
                // 400 Bad Request with validation errors
                status = (int)HttpStatusCode.BadRequest;
                title = "Doğrulama hatası";
                // Provide structured validation errors in extensions property.
                extensions["errors"] = vex.Errors ?? new Dictionary<string, string[]>();
                detail = "Bazı alanlar doğrulama kurallarına uymuyor. Lütfen alanları kontrol edin."; // User-facing Turkish message
                break;

            case NotFoundException nfe:
                // 404 Not Found
                status = (int)HttpStatusCode.NotFound;
                title = "Kayıt bulunamadı";
                detail = _env.IsDevelopment() ? nfe.Message : "İstenen kayıt bulunamadı."; // hide details in production
                break;

            case UnauthorizedAccessException _:
                // 401 Unauthorized (or 403 depending on semantics)
                status = (int)HttpStatusCode.Unauthorized;
                title = "Yetkisiz erişim";
                detail = "Bu işlemi gerçekleştirmek için yeterli izniniz yok.";
                break;

            case OperationCanceledException _:
                // 499 Client Closed Request (not standard in HttpStatusCode enum) — use 400/503. We'll use 499-like semantics with 400.
                status = (int)HttpStatusCode.BadRequest;
                title = "İşlem iptal edildi";
                detail = "İşlem kullanıcı tarafından iptal edildi veya zaman aşımı oluştu.";
                break;

            default:
                // For unexpected exceptions, do not leak server internals in production.
                if (_env.IsDevelopment())
                {
                    // In dev: include stack trace and message to help debugging
                    extensions["exception"] = new
                    {
                        type = ex.GetType().FullName,
                        message = ex.Message,
                        stackTrace = ex.StackTrace
                    };
                    detail = ex.Message;
                }
                else
                {
                    // In production: generic, user-friendly message already set above.
                }
                break;
        }

        var pd = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status,
            Instance = null // optional: could set to request path
        };

        // Add our custom fields to extensions — consumers (frontend) can use them.
        foreach (var kv in extensions)
        {
            // ProblemDetails.Extensions is IDictionary<string, object>
            pd.Extensions[kv.Key] = kv.Value;
        }

        return pd;
    }
}
