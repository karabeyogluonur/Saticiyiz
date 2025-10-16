using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ST.App.Features.Billing.Factories;
using ST.App.Features.Billing.ViewModels;
using ST.Application.Features.Billing.Commands.CreateBillingProfile;
using ST.Application.Features.Billing.Commands.DeleteBillingProfile;
using ST.Application.Features.Billing.Commands.UpdateBillingProfile;
using ST.Application.Interfaces.Messages;
using ST.Application.Wrappers;

namespace ST.App.Controllers;

public class BillingProfileController : Controller
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IBillingModelFactory _billingModelFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BillingProfileController> _logger;

    public BillingProfileController(
        IMediator mediator,
        IMapper mapper,
        IBillingModelFactory billingModelFactory,
        INotificationService notificationService,
        ILogger<BillingProfileController> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _billingModelFactory = billingModelFactory;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _billingModelFactory.PrepareListViewModelAsync();
        _logger.LogInformation("Billing profile list view prepared.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var model = await _billingModelFactory.PrepareAddViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BillingProfileAddViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await _billingModelFactory.PrepareAddViewModelAsync(model);
            return View(model);
        }

        var command = _mapper.Map<CreateBillingProfileCommand>(model);
        Response<int> response = await _mediator.Send(command);

        if (response.Succeeded)
        {
            _logger.LogInformation("New billing profile created. Profile Id: {ProfileId}", response.Data);
            await _notificationService.SuccessAsync("Fatura profili başarıyla oluşturuldu.");
            return RedirectToAction("Index");
        }

        _logger.LogWarning("Failed to create billing profile. Errors: {Errors}", response.Message);
        await _notificationService.ErrorAsync($"Fatura profili oluşturulamadı: {response.Message}");
        await _billingModelFactory.PrepareAddViewModelAsync(model);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _billingModelFactory.PrepareEditViewModelAsync(id);

        if (model.Id == 0)
        {
            _logger.LogWarning("Edit attempt with invalid Id: {Id}", id);
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BillingProfileEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await _billingModelFactory.PrepareEditViewModelAsync(model);
            return View(model);
        }

        var command = _mapper.Map<UpdateBillingProfileCommand>(model);
        Response<int> response = await _mediator.Send(command);

        if (response.Succeeded)
        {
            _logger.LogInformation("Billing profile updated successfully. Profile Id: {ProfileId}", model.Id);
            await _notificationService.SuccessAsync("Fatura profili başarıyla güncellendi.");
            return RedirectToAction("Index");
        }

        _logger.LogWarning("Failed to update billing profile. Profile Id: {ProfileId}, Errors: {Errors}", model.Id, response.Message);
        await _notificationService.ErrorAsync($"Fatura profili güncellenemedi: {response.Message}");
        await _billingModelFactory.PrepareEditViewModelAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        Response<int> response = await _mediator.Send(new DeleteBillingProfileCommand { Id = id });

        if (response.Succeeded)
        {
            _logger.LogInformation("Billing profile deleted successfully. Profile Id: {ProfileId}", id);
            return Json(new { success = true, message = "Fatura profili başarıyla silindi." });
        }

        _logger.LogWarning("Failed to delete billing profile. Profile Id: {ProfileId}, Errors: {Errors}", id, response.Message);
        return Json(new { success = false, message = $"Fatura profili silinemedi: {response.Message}" });
    }
}
