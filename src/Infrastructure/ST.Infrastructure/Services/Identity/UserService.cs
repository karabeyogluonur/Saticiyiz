using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ST.Application.DTOs.Messages;
using ST.Application.Interfaces.Configuration;
using ST.Application.Interfaces.Identity;
using ST.Application.Interfaces.Messages;
using ST.Application.Interfaces.Repositories;
using ST.Application.Settings;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;
using ST.Domain.Enums;
using ST.Infrastructure.Services.Email;


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

    public async Task<bool> IsEmailUniqueGloballyAsync(string email)
    {
        return await _unitOfWork.Users.ExistsAsync(user => user.NormalizedEmail == email.ToUpperInvariant(), ignoreQueryFilters: true);
    }

    public async Task<bool> IsPhoneUniqueGloballyAsync(string phoneNumber)
    {
        return await _unitOfWork.Users.ExistsAsync(user => user.PhoneNumber == phoneNumber, ignoreQueryFilters: true);
    }
}
