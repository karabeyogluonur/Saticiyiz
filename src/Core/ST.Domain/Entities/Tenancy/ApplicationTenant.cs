using ST.Domain.Entities.Common;
using ST.Domain.Entities.Subscriptions;
using ST.Domain.Interfaces;

namespace ST.Domain.Entities
{
    public class ApplicationTenant : BaseEntity<int>, IAuditableEntity, ISoftDeleteEntity, ITenantInfo
    {
        public ApplicationTenant() { }

        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ConnectionString { get; set; } // Boş olabilir, eğer boşsa ortak veritabanını kullanır
        public bool HasDedicatedDatabase => !string.IsNullOrEmpty(ConnectionString);
        public bool IsActive { get; set; } = true;
        public bool IsSetupCompleted { get; set; } = false;
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
