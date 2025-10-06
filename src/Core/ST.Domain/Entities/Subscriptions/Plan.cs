using ST.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST.Domain.Entities.Subscriptions
{
    public class Plan : BaseEntity<int>, IAuditableEntity, ISoftDeleteEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
    }
}