using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using ST.Application.Common.Authorization;
using ST.Infrastructure.Persistence.Contexts;
using ST.Domain.Entities;
using ST.Domain.Entities.Identity;
using Finbuckle.MultiTenant.Abstractions;

namespace ST.Infrastructure.Security.Authorization
{
    public class TenantMemberHandler : AuthorizationHandler<TenantMemberRequirement>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantMemberHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TenantMemberRequirement requirement)
        {
            ITenantInfo tenantInfo = _httpContextAccessor.HttpContext?.GetMultiTenantContext<ApplicationTenant>()?.TenantInfo;
            string userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (tenantInfo != null && userId != null)
            {
                ApplicationUser user = await _dbContext.Users
                    .AsNoTracking()
                    .OfType<ApplicationUser>()
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                bool isMember = user != null && user.TenantId == tenantInfo.Id;

                if (isMember)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}