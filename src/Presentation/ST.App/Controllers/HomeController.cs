using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ST.App.Mvc.Controllers;

namespace ST.App.Controllers;

public class HomeController : BaseMemberController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }
}
