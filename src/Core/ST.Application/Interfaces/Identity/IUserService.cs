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
    }
}
