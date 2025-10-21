using System.Security.Claims;
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

[AllowAnonymous]
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
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    #region Register

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogInformation("Authenticated user attempted to access Register page.");
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            // ✅ Kullanıcıya sadeleştirilmiş mesaj — validation hatalarını UI’da zaten gösteriyoruz.
            await _notificationService.WarningAsync("Lütfen formdaki eksik veya hatalı alanları kontrol edin.");
            return View(registerViewModel);
        }

        Response<int> response = await _mediator.Send(_mapper.Map<RegisterUserCommand>(registerViewModel));

        if (response.Succeeded)
        {
            _logger.LogInformation("New user registered successfully: {Email}", registerViewModel.Email);

            await _mediator.Send(new SendEmailVerificationCommand(registerViewModel.Email));

            await _notificationService.SuccessAsync(
                "Kaydınız başarıyla oluşturuldu! Giriş yapmadan önce e-posta adresinizi doğrulamanız gerekmektedir.");

            return RedirectToAction("Login", "Auth");
        }

        // ❌ Hata durumunda ayrıntılı log (developer için)
        _logger.LogWarning("User registration failed: {Email}. Errors: {Errors}",
            registerViewModel.Email, string.Join(", ", response.Errors ?? Array.Empty<string>()));

        // ✅ Kullanıcı dostu hata bildirimi
        await _notificationService.ErrorAsync(
            response.Message ?? "Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");

        if (response.Errors is not null)
        {
            foreach (string error in response.Errors)
                ModelState.AddModelError(string.Empty, error);
        }

        return View(registerViewModel);
    }

    #endregion

    #region Login

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogInformation("Authenticated user attempted to access Login page.");
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            await _notificationService.WarningAsync("Lütfen geçerli bir e-posta adresi ve şifre girin.");
            return View(loginViewModel);
        }

        Response<LoginUserResponseDto> result = await _mediator.Send(_mapper.Map<LoginUserCommand>(loginViewModel));

        if (result.Data is null)
        {
            _logger.LogError("Unexpected null data returned from LoginUserCommand for {Email}", loginViewModel.Email);
            await _notificationService.ErrorAsync("Giriş işlemi sırasında beklenmeyen bir hata oluştu.");
            return View(loginViewModel);
        }

        switch (result.Data.Status)
        {
            case LoginStatus.Success:
                _logger.LogInformation("User login successful: {Email}", loginViewModel.Email);
                await _notificationService.SuccessAsync("Hoş geldiniz! Başarıyla giriş yaptınız.");
                return RedirectToLocal(returnUrl);

            case LoginStatus.LockedOut:
                _logger.LogWarning("Locked account login attempt: {Email}", loginViewModel.Email);
                await _notificationService.ErrorAsync("Hesabınız çok fazla hatalı deneme nedeniyle geçici olarak kilitlendi.");
                break;

            case LoginStatus.NotAllowed:
                _logger.LogWarning("Login attempt by unverified user: {Email}", loginViewModel.Email);
                await _notificationService.WarningAsync("E-posta adresiniz doğrulanmamış. Lütfen gelen kutunuzu kontrol edin.");
                break;

            case LoginStatus.RequiresTwoFactor:
                _logger.LogInformation("Two-factor authentication required: {Email}", loginViewModel.Email);
                await _notificationService.InfoAsync("Güvenliğiniz için iki faktörlü doğrulama gereklidir.");
                break;

            case LoginStatus.InvalidCredentials:
            default:
                _logger.LogWarning("Invalid credentials attempt: {Email}", loginViewModel.Email);
                await _notificationService.ErrorAsync("E-posta adresi veya şifre hatalı. Lütfen tekrar deneyin.");
                break;
        }

        ModelState.AddModelError(string.Empty, result.Data.ErrorMessage);
        return View(loginViewModel);
    }

    #endregion

    #region Logout

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        string userName = User.Identity?.Name ?? "Unknown";

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User logged out successfully: {UserName}", userName);
        await _notificationService.SuccessAsync("Başarıyla çıkış yaptınız. Tekrar görüşmek üzere!");

        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Forgot / Reset Password

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            await _notificationService.WarningAsync("Lütfen geçerli bir e-posta adresi giriniz.");
            return View(forgotPasswordViewModel);
        }

        Response<string> response = await _mediator.Send(_mapper.Map<ForgotPasswordCommand>(forgotPasswordViewModel));

        if (!response.Succeeded)
        {
            _logger.LogWarning("Forgot password request failed for {Email}", forgotPasswordViewModel.Email);
            await _notificationService.ErrorAsync(response.Message ?? "Şifre sıfırlama bağlantısı gönderilemedi.");

            if (response.Errors is not null)
                foreach (string error in response.Errors)
                    ModelState.AddModelError(string.Empty, error);

            return View(forgotPasswordViewModel);
        }

        await _notificationService.SuccessAsync(
            "Şifre sıfırlama bağlantısı e-posta adresinize gönderilmiştir. Lütfen e-postanızı kontrol edin.");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            await _notificationService.WarningAsync("Lütfen formdaki tüm alanları doldurunuz.");
            return View(resetPasswordViewModel);
        }

        Response<string> response = await _mediator.Send(new ResetPasswordCommand
        {
            Token = resetPasswordViewModel.Token,
            NewPassword = resetPasswordViewModel.NewPassword
        });

        if (!response.Succeeded)
        {
            _logger.LogWarning("Reset password failed for token {Token}", resetPasswordViewModel.Token);
            await _notificationService.ErrorAsync(response.Message ?? "Şifre sıfırlama işlemi başarısız oldu.");

            if (response.Errors is not null)
                foreach (string error in response.Errors)
                    ModelState.AddModelError(string.Empty, error);

            return View(resetPasswordViewModel);
        }

        await _notificationService.SuccessAsync("Şifreniz başarıyla sıfırlandı. Şimdi giriş yapabilirsiniz.");
        return RedirectToAction("Login");
    }

    #endregion

    #region Verify Email / Unsubscribe

    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        Response<string> response = await _mediator.Send(new VerifyEmailCommand(token));

        if (!response.Succeeded)
        {
            _logger.LogWarning("Email verification failed. Token: {Token}", token);
            await _notificationService.ErrorAsync("E-posta doğrulama başarısız oldu. Bağlantı süresi dolmuş olabilir.");
        }
        else
        {
            _logger.LogInformation("Email verified successfully.");
            await _notificationService.SuccessAsync("E-posta adresiniz başarıyla doğrulandı. Artık giriş yapabilirsiniz!");
        }

        return View(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> Unsubscribe([FromQuery] string token)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            await _notificationService.InfoAsync("Bu işlemi yapmak için giriş yapmanız gerekmektedir.");
            string returnUrl = Url.Action("Unsubscribe", "Auth", new { token });
            return RedirectToAction("Login", "Auth", new { ReturnUrl = returnUrl });
        }

        Response<string> response = await _mediator.Send(new UnsubscribeNewsletterCommand(token));

        if (!response.Succeeded)
        {
            _logger.LogWarning("Unsubscribe operation failed. Token: {Token}", token);
            await _notificationService.ErrorAsync("Abonelikten çıkma işlemi başarısız oldu.");
        }
        else
        {
            _logger.LogInformation("User unsubscribed from newsletter successfully.");
            await _notificationService.SuccessAsync("E-posta aboneliğiniz başarıyla sonlandırıldı.");
        }

        return View(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> ResendEmailVerification(string returnUrl)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToAction("Login", "Auth");

        string? email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email claim not found for current user.");
            await _notificationService.ErrorAsync("E-posta doğrulama bağlantısı gönderilirken bir sorun oluştu.");
        }
        else
        {
            await _mediator.Send(new SendEmailVerificationCommand(email));
            _logger.LogInformation("Verification email resent to {Email}", email);
            await _notificationService.SuccessAsync($"{email} adresine doğrulama bağlantısı gönderildi.");
        }

        return RedirectToLocal(returnUrl);
    }

    #endregion

    private IActionResult RedirectToLocal(string returnUrl)
    {
        // ✅ Güvenlik: sadece local URL'lere izin verilir
        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }
}
