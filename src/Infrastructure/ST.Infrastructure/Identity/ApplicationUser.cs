using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using ST.Domain.Entities;

namespace ST.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public string TenantId { get; set; } = default!;

        [ForeignKey(nameof(TenantId))]
        public virtual ApplicationTenant Tenant { get; set; } = default!;
    }
}
