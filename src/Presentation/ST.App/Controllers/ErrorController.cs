
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ST.App.Models;

namespace ST.App.Controllers;

[AllowAnonymous]
[Route("Error")]
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        if (statusCode == 404 && statusCodeResult != null)
        {
            _logger.LogWarning("404 Not Found Error Occurred. Path: {OriginalPath}", statusCodeResult.OriginalPath);
        }

        switch (statusCode)
        {
            case 403:
                return View("AccessDenied");
            case 404:
                return View("NotFound");
            default:
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
        }
    }
}