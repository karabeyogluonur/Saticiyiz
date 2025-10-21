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
        private readonly ICurrentTenantStore _currentTenantStore;

        public LoginUserCommandHandler(
            SignInManager<ApplicationUser> signInManager,
            ITenantService tenantService,
            IUserService userService,
            ICurrentTenantStore currentTenantStore)
        {
            _signInManager = signInManager;
            _tenantService = tenantService;
            _userService = userService;
            _currentTenantStore = currentTenantStore;
        }

        public async Task<Response<LoginUserResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByEmailAsync(request.Email, ignoreQueryFilters: true);

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

            _currentTenantStore.SetTenant(user.TenantId);

            await _signInManager.SignInAsync(user, request.RememberMe);
            return Response<LoginUserResponseDto>.Success(new LoginUserResponseDto { Status = LoginStatus.Success });
        }
    }
}
