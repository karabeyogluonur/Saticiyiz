using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;

namespace ST.Application.Interfaces.Identity
{
    public interface IUserService
    {
        Task<bool> IsEmailUniqueGloballyAsync(string email);
        Task<bool> IsPhoneUniqueGloballyAsync(string phoneNumber);
        Task<(IdentityResult, ApplicationUser)> CreateUserAsync(ApplicationUser user, ApplicationTenant tenant, string password);
        Task<ApplicationUser> GetUserByIdAsync(int userId);
        Task<ApplicationUser> GetUserByEmailAsync(string userEmail, bool ignoreQueryFilters = false);
        Task<IList<Claim>> GetClaimsAsync(ApplicationUser user);
        Task RemoveClaimAsync(ApplicationUser user, Claim claim);
        Task UpdateClaimAsync(ApplicationUser user, string claimType, string newValue);
        Task AddClaimAsync(ApplicationUser user, string claimType, string claimValue);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string identityToken);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string identityToken, string newPassword);
        Task<IdentityResult> UnsubscribeFromNewsletterAsync(ApplicationUser user);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);

    }
}
