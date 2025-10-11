using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST.App.Features.Auth.ViewModels;
using ST.App.Mvc.Controllers;
using ST.Application.Features.Identity.Commands.EmailVerification;
using ST.Application.Features.Identity.Commands.ForgotPassword;
using ST.Application.Features.Identity.Commands.LoginUser;
using ST.Application.Features.Identity.Commands.RegisterUser;
using ST.Application.Features.Identity.Commands.ResetPassword;
using ST.Application.Features.Identity.Commands.UnsubscribeNewsletter;
using ST.Application.Interfaces.Messages;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.App.Controllers;

public class AuthController : BaseController
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
    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        if (ModelState.IsValid)
        {
            Response<int> response = await _mediator.Send(_mapper.Map<RegisterUserCommand>(registerViewModel));

            if (response.Succeeded)
            {
                await _mediator.Send(new SendEmailVerificationCommand(registerViewModel.Email));

                _logger.LogInformation("Yeni kullanıcı kaydı başarılı: {Email}", registerViewModel.Email);
                await _notificationService.SuccessAsync(
                    "Kaydınız başarıyla oluşturuldu. E-posta adresinize gelen mailden doğrulamayı yaptıktan sonra giriş yapabilirsiniz.");
                return RedirectToAction(nameof(Login));
            }
            else
            {
                _logger.LogWarning("Kullanıcı kaydı başarısız oldu: {Email}. Hatalar: {Errors}",
                    registerViewModel.Email, string.Join(", ", response.Errors));
                await _notificationService.ErrorAsync(response.Message);
                if (response.Errors is not null)
                {
                    foreach (string error in response.Errors)
                        ModelState.AddModelError(string.Empty, error);
                }
            }
        }

        return View(registerViewModel);
    }

    #endregion

    #region Login

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            Response<LoginResultDto> result = await _mediator.Send(_mapper.Map<LoginUserCommand>(loginViewModel));

            if (result.Data != null)
            {
                switch (result.Data.Status)
                {
                    case LoginStatus.Success:
                        _logger.LogInformation("Kullanıcı girişi başarılı: {Email}", loginViewModel.Email);
                        await _notificationService.SuccessAsync("Kullanıcı girişi başarılı!");
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
                        _logger.LogWarning("Giriş izni olmayan hesap için deneme (örn: e-posta onaysız): {Email}",
                            loginViewModel.Email);
                        ModelState.AddModelError(string.Empty, result.Data.ErrorMessage);
                        break;

                    case LoginStatus.RequiresTwoFactor:
                        _logger.LogInformation("Kullanıcı için iki faktörlü kimlik doğrulama gerekiyor: {Email}",
                            loginViewModel.Email);
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
                _logger.LogError("LoginUserCommand işlenirken beklenmedik bir hata oluştu. Mesaj: {Message}",
                    result.Message);
                ModelState.AddModelError(string.Empty, "Giriş işlemi sırasında beklenmedik bir hata oluştu.");
            }
        }
        return View(loginViewModel);
    }

    #endregion

    #region Logout
    public async Task<IActionResult> Logout()
    {
        string userName = User.Identity.Name;

        await _signInManager.SignOutAsync();

        _logger.LogInformation("Kullanıcı çıkış yaptı: {UserName}", userName);

        await _notificationService.SuccessAsync("Başarıyla çıkış yapıldı!");

        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region ForgotPassword

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
    {
        if (!ModelState.IsValid)
            return View(forgotPasswordViewModel);

        Response<string> response = await _mediator.Send(_mapper.Map<ForgotPasswordCommand>(forgotPasswordViewModel));

        if (!response.Succeeded)
        {
            await _notificationService.ErrorAsync(response.Message);

            if (response.Errors is not null)
            {
                foreach (string error in response.Errors)
                    ModelState.AddModelError(string.Empty, error);
            }
            return View(forgotPasswordViewModel);
        }

        await _notificationService.SuccessAsync(response.Message);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        ResetPasswordViewModel model = new ResetPasswordViewModel { Token = token };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        if (!ModelState.IsValid)
            return View(resetPasswordViewModel);

        Response<string> response = await _mediator.Send(new ResetPasswordCommand
        {
            Token = resetPasswordViewModel.Token,
            NewPassword = resetPasswordViewModel.NewPassword
        });

        if (!response.Succeeded)
        {
            await _notificationService.ErrorAsync(response.Message);

            if (response.Errors is not null)
            {
                foreach (string error in response.Errors)
                    ModelState.AddModelError(string.Empty, error);
            }

            return View(resetPasswordViewModel);
        }

        await _notificationService.SuccessAsync(response.Message);
        return RedirectToAction("Login");
    }

    #endregion

    #region Verify Email & Unsubscribe Newsletter

    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        Response<string> response = await _mediator.Send(new VerifyEmailCommand(token));

        if (!response.Succeeded)
            await _notificationService.ErrorAsync(response.Message);
        else
            await _notificationService.SuccessAsync(response.Message);

        return View(nameof(Login));
    }
    [HttpGet]
    public async Task<IActionResult> Unsubscribe([FromQuery] string token)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            await _notificationService.WarningAsync(
                "Bu işlemi gerçekleştirmek için giriş yapmalısınız.");

            string returnUrl = Url.Action("Unsubscribe", "Auth", new { token });
            return RedirectToAction("Login", "Auth", new { ReturnUrl = returnUrl });
        }

        Response<string> response = await _mediator.Send(new UnsubscribeNewsletterCommand(token));

        if (!response.Succeeded)
            await _notificationService.ErrorAsync(response.Message);
        else
            await _notificationService.SuccessAsync(response.Message);

        return View(nameof(Login));
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
