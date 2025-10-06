using ST.Domain.Entities.Common;

namespace ST.Domain.Entities.Configurations
{
    public class Setting : BaseEntity<int>, IAuditableEntity, ISoftDeleteEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public string? TenantId { get; set; }

        public string CreatedBy { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
    }
}