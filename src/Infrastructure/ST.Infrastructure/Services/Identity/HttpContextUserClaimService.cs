using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ST.Application.Interfaces.Identity;

namespace ST.Infrastructure.Services.Identity
{
    public class HttpContextUserClaimService : IHttpContextUserClaimService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextUserClaimService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddOrUpdateClaimAsync(string claimType, string claimValue)
        {
            await Task.Run(() =>
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity is ClaimsIdentity identity)
                {
                    var existingClaim = identity.FindFirst(claimType);
                    if (existingClaim != null)
                        identity.RemoveClaim(existingClaim);

                    identity.AddClaim(new Claim(claimType, claimValue));
                }
            });
        }

        public async Task RemoveClaimAsync(string claimType)
        {
            await Task.Run(() =>
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity is ClaimsIdentity identity)
                {
                    var existingClaim = identity.FindFirst(claimType);
                    if (existingClaim != null)
                        identity.RemoveClaim(existingClaim);
                }
            });
        }

        public async Task<string?> GetClaimValueAsync(string claimType)
        {
            return await Task.Run(() =>
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(claimType)?.Value;
            });
        }
    }
}
