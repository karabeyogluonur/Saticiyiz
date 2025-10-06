using Microsoft.AspNetCore.Identity;

namespace ST.Infrastructure.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string TenantId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}