using ST.Domain.Entities.Common;
using ST.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ST.Domain.Entities; // Plan ve ApplicationTenant için

namespace ST.Domain.Entities.Subscriptions
{
    public class Subscription : BaseEntity<Guid>, IAuditableEntity, ISoftDeleteEntity
    {
        public string TenantId { get; set; } = default!;

        public int PlanId { get; set; }

        // Abonelik Bilgileri
        public SubscriptionStatus Status { get; set; }
        public DateTime CurrentPeriodEndDate { get; set; }

        public virtual ApplicationTenant Tenant { get; set; } = default!;

        public virtual Plan Plan { get; set; } = default!;

        // IAuditableEntity Uygulaması
        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // ISoftDeleteEntity Uygulaması
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
    }
}
