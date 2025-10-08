using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ST.App.Mvc.Controllers;

namespace ST.App.Controllers;

public class SetupController : Controller
{
    public SetupController()
    {

    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteSetup()
    {
        return View();
    }
}
