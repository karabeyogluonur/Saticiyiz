using Microsoft.AspNetCore.Mvc;
using ST.Application.Wrappers;
using ST.Application.Interfaces.Messages;

namespace ST.App.Mvc.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly INotificationService _notificationService;
        protected readonly ILogger<BaseController> _logger;

        protected BaseController() { }

        protected BaseController(INotificationService notificationService, ILogger<BaseController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        protected async Task<IActionResult> HandleResponse<T>(
            Response<T> response,
            string successRedirectAction = null,
            string successRedirectController = null,
            object routeValues = null,
            string viewName = null,
            object viewModel = null)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (response == null)
            {
                _logger.LogError("HandleResponse: Response nesnesi null döndü.");
                if (isAjax)
                    return Json(new { success = false, message = "Beklenmeyen bir hata oluştu." });

                await _notificationService.ErrorAsync("Beklenmeyen bir hata oluştu.");
                return RedirectToAction("Index", "Home");
            }

            // 🧠 JSON/AJAX cevabı
            if (isAjax)
            {
                return Json(new
                {
                    success = response.Succeeded,
                    message = response.Message,
                    errors = response.Errors,
                    data = response.Data
                });
            }

            // ✅ Başarılı işlem
            if (response.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(response.Message))
                    await _notificationService.SuccessAsync(response.Message);

                if (!string.IsNullOrEmpty(successRedirectAction))
                {
                    return string.IsNullOrEmpty(successRedirectController)
                        ? RedirectToAction(successRedirectAction, routeValues)
                        : RedirectToAction(successRedirectAction, successRedirectController, routeValues);
                }

                return RedirectToAction("Index", ControllerContext.ActionDescriptor.ControllerName);
            }

            // ❌ Hatalı işlem

            if (!string.IsNullOrWhiteSpace(response.Message))
                await _notificationService.ErrorAsync(response.Message);

            if (response.Errors != null)
            {
                foreach (string error in response.Errors)
                    ModelState.AddModelError(string.Empty, error);
            }

            if (viewModel != null)
                return View(viewName ?? ControllerContext.ActionDescriptor.ActionName, viewModel);

            return View(viewName ?? ControllerContext.ActionDescriptor.ActionName);
        }
    }
}
