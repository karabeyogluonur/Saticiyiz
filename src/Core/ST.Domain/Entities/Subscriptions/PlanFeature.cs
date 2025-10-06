using ST.Domain.Entities.Common;
using ST.Domain.Entities.Subscriptions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST.Domain.Entities.Subscriptions
{
    public class PlanFeature : BaseEntity<int>
    {
        public int PlanId { get; set; }
        public int FeatureDefinitionId { get; set; }

        public string Value { get; set; } = string.Empty;
        public virtual Plan Plan { get; set; } = default!;

        [ForeignKey(nameof(FeatureDefinitionId))]
        public virtual FeatureDefinition FeatureDefinition { get; set; } = default!;
    }
}