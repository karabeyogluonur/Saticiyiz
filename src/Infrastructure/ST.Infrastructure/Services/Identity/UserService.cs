using Microsoft.AspNetCore.Identity;
using ST.Application.Interfaces.Identity;
using ST.Infrastructure.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ST.Infrastructure.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(IdentityResult Result, string UserId)> CreateUserWithTenantAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string tenantId)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                TenantId = tenantId
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                return (result, user.Id);
            }
            return (result, string.Empty);
        }

        public async Task AddUserToTenantRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
        public async Task<string?> CheckPasswordSignInAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !user.IsActive)
            {
                return null;
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return user.Id;
            }

            return null;
        }

        public async Task<string?> GetTenantIdByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.TenantId;
        }

        public async Task<IList<string>> GetRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }
            return await _userManager.GetRolesAsync(user);
        }
    }
}