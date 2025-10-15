using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST.Application.Features.Tenancy.Commands.SetupTenant;
using ST.App.Mvc.Controllers;
using System.Threading.Tasks;
using ST.Application.Interfaces.Messages;
using ST.Application.Wrappers;
using Microsoft.AspNetCore.Identity;
using ST.Domain.Entities.Identity;

namespace ST.App.Controllers
{
    [Authorize]
    public class SetupController : BaseMemberController
    {
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        public SetupController(IMediator mediator, INotificationService notificationService)
        {
            _mediator = mediator;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            Response<int> response = await _mediator.Send(new SetupTenantCommand());

            if (response.Succeeded)
            {
                await _notificationService.SuccessAsync(response.Message);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                await _notificationService.ErrorAsync(response.Message);
                return RedirectToAction("Login", "Auth");
            }
        }
    }
}