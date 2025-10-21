using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST.App.Mvc.Controllers;
using ST.Application.Features.Tenancy.Commands.SetupTenant;
using ST.Application.Interfaces.Messages;
using ST.Application.Wrappers;

namespace ST.App.Controllers
{
    [Authorize]
    public class SetupController : BaseMemberController
    {
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SetupController> _logger;

        public SetupController(
            IMediator mediator,
            INotificationService notificationService,
            ILogger<SetupController> logger)
        {
            _mediator = mediator;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Response<int> response = await _mediator.Send(new SetupTenantCommand());

            if (response.Succeeded)
            {
                _logger.LogInformation("Tenant setup completed successfully. Tenant Id: {TenantId}", response.Data);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogWarning("Tenant setup failed. Errors: {Errors}", response.Message);
                await _notificationService.ErrorAsync($"Proje kurulumu sırasında bir hata oluştu: {response.Message}");
                return RedirectToAction("Login", "Auth");
            }
        }
    }
}
