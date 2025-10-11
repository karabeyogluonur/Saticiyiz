using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;
using System.Security.Claims;

namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Response<LoginResultDto>>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public LoginUserCommandHandler(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<LoginResultDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.LockedOut, ErrorMessage = "Hesabınız çok fazla başarısız deneme nedeniyle kilitlenmiştir. Lütfen daha sonra tekrar deneyin." });

            if (result.IsNotAllowed)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınıza giriş izni yok. Lütfen e-postanızı onayladığınızdan emin olun." });

            if (result.RequiresTwoFactor)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.RequiresTwoFactor });

            if (!result.Succeeded)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            var tenant = await _unitOfWork.Tenants.GetByIdAsync(user.TenantId);

            if (tenant == null)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınızla ilişkili bir şirket bilgisi bulunamadı. Lütfen destek ile iletişime geçin." });

            var claims = new List<Claim>
            {
                new Claim(CustomClaims.TenantId, user.TenantId.ToString())
            };

            await _signInManager.SignInWithClaimsAsync(user, request.RememberMe, claims);

            var resultDto = new LoginResultDto
            {
                Status = LoginStatus.Success,
                RequiresSetup = !tenant.IsSetupComplete
            };

            return Response<LoginResultDto>.Success(resultDto);
        }
    }
}
