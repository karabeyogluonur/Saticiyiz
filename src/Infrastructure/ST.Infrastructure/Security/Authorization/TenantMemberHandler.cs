using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ST.Infrastructure.Persistence;
using System.Security.Claims;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using ST.Application.Common.Authorization;
using ST.Infrastructure.Tenancy;
using ST.Infrastructure.Identity;
using ST.Infrastructure.Persistence.Contexts;
using ST.Domain.Entities;

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
            var tenantInfo = _httpContextAccessor.HttpContext?.GetMultiTenantContext<ApplicationTenant>()?.TenantInfo;
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (tenantInfo != null && userId != null)
            {
                var user = await _dbContext.Users
                    .AsNoTracking()
                    .OfType<ApplicationUser>()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                bool isMember = user != null && user.TenantId == tenantInfo.Id;

                if (isMember)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}