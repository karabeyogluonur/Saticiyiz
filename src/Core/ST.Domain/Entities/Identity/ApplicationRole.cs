using Microsoft.AspNetCore.Identity;

namespace ST.Domain.Entities.Identity
{
    public class ApplicationRole : IdentityRole<int>
    {
        public string TenantId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}