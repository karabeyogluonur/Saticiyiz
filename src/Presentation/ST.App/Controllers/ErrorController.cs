using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ST.App.Features.Errors.ViewModels;

namespace ST.App.Controllers;

/// <summary>
/// Central error handling controller.
/// Handles HTTP status codes (404, 403, etc.) and displays user-friendly views.
/// Logs contextual information for monitoring and debugging.
/// </summary>
[Route("Error")]
public class ErrorController : Controller // BaseController bağımlılığı kaldırıldı; isteğe bağlı eklenebilir
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles specific HTTP status codes (404, 403, etc.)
    /// Logs warnings and provides user-friendly views/messages.
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    [Route("{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        // Get feature for original request path
        var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        // TraceId for correlation in logs and APM
        string traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        string originalPath = statusCodeFeature?.OriginalPath ?? "Unknown";

        // Log based on status code
        switch (statusCode)
        {
            case 404:
                _logger.LogWarning(
                    "404 Not Found Error. Path: {OriginalPath}, Query: {QueryString}, TraceId: {TraceId}",
                    originalPath,
                    statusCodeFeature?.OriginalQueryString,
                    traceId
                );
                // Return user-friendly view
                return View("NotFound", new ErrorViewModel
                {
                    RequestId = traceId,
                    Message = "Üzgünüz, aradığınız sayfa bulunamadı."
                });

            case 403:
                _logger.LogWarning(
                    "403 Forbidden Error. Path: {OriginalPath}, Query: {QueryString}, TraceId: {TraceId}",
                    originalPath,
                    statusCodeFeature?.OriginalQueryString,
                    traceId
                );
                return View("AccessDenied", new ErrorViewModel
                {
                    RequestId = traceId,
                    Message = "Bu sayfaya erişim yetkiniz bulunmamaktadır."
                });

            default:
                _logger.LogError(
                    "Unhandled HTTP status code {StatusCode}. Path: {OriginalPath}, Query: {QueryString}, TraceId: {TraceId}",
                    statusCode,
                    originalPath,
                    statusCodeFeature?.OriginalQueryString,
                    traceId
                );
                return View("Error", new ErrorViewModel
                {
                    RequestId = traceId,
                    Message = "Beklenmedik bir hata oluştu. Lütfen tekrar deneyiniz."
                });
        }
    }

    /// <summary>
    /// Optional: default action for general errors (unhandled exceptions)
    /// Can be mapped from UseExceptionHandler("/Error") in Program.cs
    /// </summary>
    [Route("")]
    public IActionResult Index()
    {
        string traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogError("Unhandled exception caught in ErrorController. TraceId: {TraceId}", traceId);

        return View("Error", new ErrorViewModel
        {
            RequestId = traceId,
            Message = "Beklenmedik bir hata oluştu. Lütfen tekrar deneyiniz."
        });
    }
}
