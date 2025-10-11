using Microsoft.AspNetCore.Identity;
using ST.Domain.Entities.Common;

namespace ST.Domain.Entities.Identity
{
    public class ApplicationRole : IdentityRole<int>, ITenantEntity
    {

        public string Description { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public virtual ApplicationTenant Tenant { get; set; }
    }
}
