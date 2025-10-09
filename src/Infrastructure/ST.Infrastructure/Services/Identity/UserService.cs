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
    }
}