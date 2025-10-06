using Microsoft.AspNetCore.Identity;

namespace ST.Infrastructure.Identity
{
    public class ApplicationRole : IdentityRole // Finbuckle'dan türetin
    {
        public string TenantId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}