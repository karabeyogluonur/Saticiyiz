using Microsoft.AspNetCore.Identity;

namespace ST.Domain.Entities.Identity
{
    public class ApplicationUserClaim : IdentityUserClaim<int>
    {
        public string TenantId { get; set; } = string.Empty;
    }
}
