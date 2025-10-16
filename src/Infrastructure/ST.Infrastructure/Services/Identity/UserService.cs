using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;


namespace ST.Infrastructure.Services.Identity;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    public UserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }
    public async Task<(IdentityResult, ApplicationUser)> CreateUserAsync(ApplicationUser user, ApplicationTenant tenant, string password)
    {
        user.Tenant = tenant;

        IdentityResult identityResult = await _userManager.CreateAsync(user, password);

        return (identityResult, identityResult.Succeeded ? user : null);
    }

    public async Task<ApplicationUser> GetUserByIdAsync(int userId)
    {
        return await _unitOfWork.Users.FindAsync(userId);
    }

    public async Task<bool> IsEmailUniqueGloballyAsync(string email)
    {
        bool emailExist = await _unitOfWork.Users.ExistsAsync(user => user.NormalizedEmail == email.ToUpperInvariant(), ignoreQueryFilters: true);
        return !emailExist;
    }

    public async Task<bool> IsPhoneUniqueGloballyAsync(string phoneNumber)
    {
        bool phoneExist = await _unitOfWork.Users.ExistsAsync(user => user.PhoneNumber == phoneNumber, ignoreQueryFilters: true);
        return !phoneExist;
    }
    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        return await _userManager.GetClaimsAsync(user);
    }
    public async Task AddClaimAsync(ApplicationUser user, string claimType, string claimValue)
    {
        var claim = new Claim(claimType, claimValue);
        await _userManager.AddClaimAsync(user, claim);
    }

    public async Task AddClaimAsync(ApplicationUser user, Claim claim)
    {
        await _userManager.AddClaimAsync(user, claim);
    }
    public async Task UpdateClaimAsync(ApplicationUser user, string claimType, string newValue)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var existingClaim = claims.FirstOrDefault(claim => claim.Type == claimType);

        if (existingClaim != null)
            await _userManager.RemoveClaimAsync(user, existingClaim);

        await _userManager.AddClaimAsync(user, new Claim(claimType, newValue));

    }
    public async Task RemoveClaimAsync(ApplicationUser user, Claim claim)
    {
        await _userManager.RemoveClaimAsync(user, claim);
    }

    public async Task<ApplicationUser> GetUserByEmailAsync(string userEmail, bool ignoreQueryFilters = false)
    {
        return await _unitOfWork.Users.GetFirstOrDefaultAsync(predicate: user => user.Email == userEmail, ignoreQueryFilters: ignoreQueryFilters);
    }
    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string identityToken)
    {
        return await _userManager.ConfirmEmailAsync(user, identityToken);
    }
    public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string identityToken, string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, identityToken, newPassword);
    }
    public async Task<IdentityResult> UnsubscribeFromNewsletterAsync(ApplicationUser user)
    {
        user.IsSubscribedToNewsletter = false;
        user.NewsletterSubscribedAt = null;
        return await _userManager.UpdateAsync(user);
    }
}
