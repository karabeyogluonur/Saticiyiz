using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST.App.Features.Auth.ViewModels;
using ST.Application.Features.Identity.Commands.LoginUser;
using ST.Application.Features.Identity.Commands.RegisterUser;
using ST.Application.Interfaces.Messages;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.App.Controllers;

public class AuthController : Controller
{
    private readonly IMediator _mediator;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthController> _logger;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public AuthController(
        IMediator mediator,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthController> logger,
        IMapper mapper,
        INotificationService notificationService)
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _logger = logger;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    #region Register

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        if (ModelState.IsValid)
        {
            Response<int> result = await _mediator.Send(_mapper.Map<RegisterUserCommand>(registerViewModel));

            if (result.Succeeded)
            {
                _logger.LogInformation("Yeni kullanıcı kaydı başarılı: {Email}", registerViewModel.Email);
                await _notificationService.SuccessAsync("Kaydınız başarıyla oluşturuldu. Lütfen giriş yapınız.");
                return RedirectToAction(nameof(Login));
            }
            else
            {
                _logger.LogWarning("Kullanıcı kaydı başarısız oldu: {Email}. Hatalar: {Errors}", registerViewModel.Email, string.Join(", ", result.Errors));
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
        }

        return View(registerViewModel);
    }

    #endregion

    #region Login

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            Response<LoginResultDto> result = await _mediator.Send(_mapper.Map<LoginUserCommand>(loginViewModel));

            if (result.Succeeded && result.Data != null)
            {
                switch (result.Data.Status)
                {
                    case LoginStatus.Success:
                        _logger.LogInformation("Kullanıcı girişi başarılı: {Email}", loginViewModel.Email);
                        if (result.Data.RequiresSetup)
                        {
                            return RedirectToAction("Index", "Setup");
                        }
                        return RedirectToLocal(returnUrl);

                    case LoginStatus.LockedOut:
                        _logger.LogWarning("Kilitlenmiş hesap için giriş denemesi: {Email}", loginViewModel.Email);
                        ModelState.AddModelError(string.Empty, result.Data.ErrorMessage);
                        break;

                    case LoginStatus.NotAllowed:
                        _logger.LogWarning("Giriş izni olmayan hesap için deneme (örn: e-posta onaysız): {Email}", loginViewModel.Email);
                        ModelState.AddModelError(string.Empty, result.Data.ErrorMessage);
                        break;

                    case LoginStatus.RequiresTwoFactor:
                        _logger.LogInformation("Kullanıcı için iki faktörlü kimlik doğrulama gerekiyor: {Email}", loginViewModel.Email);

                        ModelState.AddModelError(string.Empty, "İki faktörlü kimlik doğrulama gereklidir.");
                        break;

                    case LoginStatus.InvalidCredentials:
                    default:
                        _logger.LogWarning("Geçersiz şifre denemesi: {Email}", loginViewModel.Email);
                        ModelState.AddModelError(string.Empty, result.Data.ErrorMessage);
                        break;
                }
            }
            else
            {
                _logger.LogError("LoginUserCommand işlenirken beklenmedik bir hata oluştu. Mesaj: {Message}", result.Message);
                ModelState.AddModelError(string.Empty, "Giriş işlemi sırasında beklenmedik bir hata oluştu.");
            }
        }
        return View(loginViewModel);
    }

    #endregion

    #region Logout

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity.Name;
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Kullanıcı çıkış yaptı: {UserName}", userName);
        return RedirectToAction("Index", "Home");
    }

    #endregion

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
}