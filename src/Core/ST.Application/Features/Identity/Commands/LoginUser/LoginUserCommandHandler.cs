using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;
using System.Security.Claims;

namespace ST.Application.Features.Identity.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Response<LoginResultDto>>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ApplicationTenant> _applicationTenantRepository;
        private readonly IRepository<ApplicationUserClaim> _applicationUserClaimRepository;

        public LoginUserCommandHandler(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _applicationTenantRepository = _unitOfWork.GetRepository<ApplicationTenant>();
            _applicationUserClaimRepository = _unitOfWork.GetRepository<ApplicationUserClaim>();
        }

        public async Task<Response<LoginResultDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.LockedOut, ErrorMessage = "Hesabınız çok fazla başarısız deneme nedeniyle kilitlenmiştir. Lütfen daha sonra tekrar deneyin." });

            if (result.IsNotAllowed)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınıza giriş izni yok. Lütfen e-postanızı onayladığınızdan emin olun." });

            if (result.RequiresTwoFactor)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.RequiresTwoFactor });

            if (!result.Succeeded)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.InvalidCredentials, ErrorMessage = "Geçersiz e-posta veya şifre." });

            IRepository<ApplicationTenant> tenantRepo = _unitOfWork.GetRepository<ApplicationTenant>();

            ApplicationTenant tenant = await tenantRepo.FindAsync(user.TenantId);

            if (tenant == null)
                return Response<LoginResultDto>.ErrorWithData(new LoginResultDto { Status = LoginStatus.NotAllowed, ErrorMessage = "Hesabınızla ilişkili şirket bilgisi bulunamadı." });

            List<Claim> userClaims = await GetUserClaimsIgnoringFiltersAsync(user.Id, tenant.Id);

            if (userClaims is not null)
            {
                Claim oldSetupClaim = userClaims.FirstOrDefault(c => c.Type == CustomClaims.IsSetupComplete);

                if (oldSetupClaim != null)
                    await _userManager.RemoveClaimAsync(user, oldSetupClaim);
            }

            List<Claim> claims = new List<Claim> { new Claim(CustomClaims.IsSetupComplete, tenant.IsSetupComplete.ToString()) };

            await _signInManager.SignInWithClaimsAsync(user, true, claims);

            LoginResultDto resultDto = new LoginResultDto { Status = LoginStatus.Success, RequiresSetup = !tenant.IsSetupComplete };

            return Response<LoginResultDto>.Success(resultDto);
        }
        public async Task<List<Claim>> GetUserClaimsIgnoringFiltersAsync(int userId, string tenantId)
        {
            IList<ApplicationUserClaim> userClaims = await _applicationUserClaimRepository.GetAllAsync(predicate: userClaim => userClaim.UserId == userId && userClaim.TenantId == tenantId, ignoreQueryFilters: true);
            if (userClaims is not null)
                return userClaims.Select(uc => new Claim(uc.ClaimType, uc.ClaimValue)).ToList();
            else
                return null;
        }

    }
}