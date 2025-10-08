using Microsoft.AspNetCore.Identity;

namespace ST.Domain.Entities.Identity
{
    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public string TenantId { get; set; } = string.Empty;
    }
}