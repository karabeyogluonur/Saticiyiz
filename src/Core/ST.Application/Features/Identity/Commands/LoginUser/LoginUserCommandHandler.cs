using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Application.Interfaces.Tenancy;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;
using System.Security.Claims;

namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Response<LoginUserResponseDto>>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;

        public LoginUserCommandHandler(
            SignInManager<ApplicationUser> signInManager,
            ITenantService tenantService,
            IUserService userService)
        {
            _signInManager = signInManager;
            _tenantService = tenantService;
            _userService = userService;
        }

        public async Task<Response<LoginUserResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
                return Response<LoginUserResponseDto>.ErrorWithData(new LoginUserResponseDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return Response<LoginUserResponseDto>.ErrorWithData(new LoginUserResponseDto { Status = LoginStatus.LockedOut, ErrorMessage = "Hesabınız çok fazla başarısız deneme nedeniyle kilitlenmiştir. Lütfen daha sonra tekrar deneyin." });

            if (result.IsNotAllowed)
                return Response<LoginUserResponseDto>.ErrorWithData(new LoginUserResponseDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınıza giriş izni yok. Lütfen yöneticiler ile iletişime geçiniz!" });

            if (result.RequiresTwoFactor)
                return Response<LoginUserResponseDto>.ErrorWithData(new LoginUserResponseDto { Status = LoginStatus.RequiresTwoFactor });

            if (!result.Succeeded)
                return Response<LoginUserResponseDto>.ErrorWithData(new LoginUserResponseDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            var tenant = await _tenantService.GetTenantByIdAsync(user.TenantId);

            if (tenant == null)
                return Response<LoginUserResponseDto>.ErrorWithData(new LoginUserResponseDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınızla ilişkili bir şirket bilgisi bulunamadı. Lütfen destek ile iletişime geçin." });

            var claims = new List<Claim>
            {
                new Claim(CustomClaims.TenantId, user.TenantId.ToString()),
                new Claim(CustomClaims.EmailVerification, user.EmailConfirmed.ToString()),
                new Claim(CustomClaims.IsSetupCompleted, tenant.IsSetupCompleted.ToString())
            };

            await _signInManager.SignInWithClaimsAsync(user, request.RememberMe, claims);
            return Response<LoginUserResponseDto>.Success(new LoginUserResponseDto { Status = LoginStatus.Success });
        }
    }
}
