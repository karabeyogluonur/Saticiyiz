using Microsoft.AspNetCore.Identity;
using ST.Application.Wrappers;
using ST.Domain.Entities.Identity;

namespace ST.Application.Interfaces.Identity
{
    public interface IUserService
    {
        Task<Response<int>> CreateUserAsync(ApplicationUser user, string password, string tenantId);
    }
}