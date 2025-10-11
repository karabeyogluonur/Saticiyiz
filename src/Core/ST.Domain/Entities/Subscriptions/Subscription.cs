using ST.Domain.Entities.Common;
using ST.Domain.Enums;

namespace ST.Domain.Entities.Subscriptions
{
    public class Subscription : BaseEntity<int>, IAuditableEntity, ISoftDeleteEntity, ITenantEntity
    {
        public int PlanId { get; set; }
        public int TenantId { get; set; }
        public virtual ApplicationTenant Tenant { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTime CurrentPeriodEndDate { get; set; }

        public virtual Plan Plan { get; set; } = default!;

        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
    }
}
