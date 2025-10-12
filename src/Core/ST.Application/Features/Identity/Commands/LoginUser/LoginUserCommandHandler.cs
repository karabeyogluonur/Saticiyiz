using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;
using System.Security.Claims;

namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Response<LoginUserResultDto>>
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

        public async Task<Response<LoginUserResultDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
                return Response<LoginUserResultDto>.ErrorWithData(new LoginUserResultDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return Response<LoginUserResultDto>.ErrorWithData(new LoginUserResultDto { Status = LoginStatus.LockedOut, ErrorMessage = "Hesabınız çok fazla başarısız deneme nedeniyle kilitlenmiştir. Lütfen daha sonra tekrar deneyin." });

            if (result.IsNotAllowed)
                return Response<LoginUserResultDto>.ErrorWithData(new LoginUserResultDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınıza giriş izni yok. Lütfen yöneticiler ile iletişime geçiniz!" });

            if (result.RequiresTwoFactor)
                return Response<LoginUserResultDto>.ErrorWithData(new LoginUserResultDto { Status = LoginStatus.RequiresTwoFactor });

            if (!result.Succeeded)
                return Response<LoginUserResultDto>.ErrorWithData(new LoginUserResultDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            var tenant = await _unitOfWork.Tenants.FindAsync(user.TenantId);

            if (tenant == null)
                return Response<LoginUserResultDto>.ErrorWithData(new LoginUserResultDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınızla ilişkili bir şirket bilgisi bulunamadı. Lütfen destek ile iletişime geçin." });

            var claims = new List<Claim>
            {
                new Claim(CustomClaims.TenantId, user.TenantId.ToString()),
                new Claim(CustomClaims.EmailVerification, user.EmailConfirmed.ToString())
            };

            await _signInManager.SignInWithClaimsAsync(user, request.RememberMe, claims);

            var resultDto = new LoginUserResultDto
            {
                Status = LoginStatus.Success,
            };

            return Response<LoginUserResultDto>.Success(resultDto);
        }
    }
}
