using ST.Domain.Entities.Common;

namespace ST.Domain.Entities.Configurations
{
    // Temel bir Ayar (Setting) varlığı
    public class Setting : BaseEntity<int>, IAuditableEntity, ISoftDeleteEntity
    {
        public string Key { get; set; } = string.Empty; // Örn: "TrialPeriodDays"
        public string Value { get; set; } = string.Empty; // Örn: "7"
        public string Type { get; set; } = string.Empty; // Örn: "int"

        // Global ayar ise NULL kalır
        public string? TenantId { get; set; }

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