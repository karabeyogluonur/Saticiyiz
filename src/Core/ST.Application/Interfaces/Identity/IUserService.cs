using Microsoft.AspNetCore.Identity;

namespace ST.Application.Interfaces.Identity
{
    public interface IUserService
    {
        Task<(IdentityResult Result, string UserId)> CreateUserWithTenantAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string tenantId);

        Task AddUserToTenantRoleAsync(string userId, string roleName);

        Task<string?> GetTenantIdByUserIdAsync(string userId);

        Task<string?> CheckPasswordSignInAsync(string email, string password);

        Task<IList<string>> GetRolesAsync(string userId);
    }
}