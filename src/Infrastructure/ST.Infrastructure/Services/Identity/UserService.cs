using Microsoft.AspNetCore.Identity;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Infrastructure.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ApplicationUserClaim> _applicationUserClaimRepository;

        public UserService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<int>> CreateUserAsync(ApplicationUser user, string password, string tenantId)
        {
            ApplicationUser userWithSameEmail = await _userManager.FindByEmailAsync(user.Email);

            if (userWithSameEmail != null)
                return new Response<int>($"'{user.Email}' e-posta adresi zaten kullanılıyor.");

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return new Response<int>("Kullanıcı oluşturulurken bir hata oluştu.", result.Errors.Select(e => e.Description));

            return new Response<int>(user.Id, "Kullanıcı başarıyla oluşturuldu.");
        }
    }
}