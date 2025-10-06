using ST.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST.Domain.Entities.Subscriptions
{
    public class FeatureDefinition : BaseEntity<int>, IAuditableEntity, ISoftDeleteEntity
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string FeatureType { get; set; } = "Limit";
        public virtual ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();

        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
    }
}