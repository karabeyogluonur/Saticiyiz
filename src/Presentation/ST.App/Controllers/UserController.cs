using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ST.App.Features.Billing.Factories;
using ST.App.Features.Billing.ViewModels;
using ST.App.Mvc.Controllers;
using ST.Application.Features.Billing.Commands;
using ST.Application.Interfaces.Messages;
using ST.Application.Wrappers;

namespace ST.App.Controllers;

public class UserController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IBillingModelFactory _billingModelFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IMediator mediator,
        IMapper mapper,
        IBillingModelFactory billingModelFactory,
        INotificationService notificationService,
        ILogger<UserController> logger)
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
        return View(model);
    }
}