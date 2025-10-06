using Finbuckle.MultiTenant.Abstractions;
using ST.Domain.Entities.Common;
using ST.Domain.Entities.Subscriptions; // Navigation için User Entity

namespace ST.Domain.Entities
{
    public class ApplicationTenant : BaseEntity<string>, IAuditableEntity, ISoftDeleteEntity, ITenantInfo
    {
        public ApplicationTenant() { }

        public ApplicationTenant(string id, string identifier, string name, string? connectionString)
        {
            this.Id = id;
            this.Identifier = identifier;
            this.Name = name;
            this.ConnectionString = connectionString;
        }

        string? ITenantInfo.Id
        {
            get => this.Id;
            set => this.Id = value ?? throw new ArgumentNullException(nameof(value), "Tenant Id cannot be null.");
        }

        public void Set_Id(string id) => this.Id = id;

        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ConnectionString { get; set; }
        public bool IsActive { get; set; } = true;

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
