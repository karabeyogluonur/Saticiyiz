using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using ST.Domain.Events.Common;

namespace ST.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser<int>, IDomainEvent
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string TenantId { get; set; } = default!;

        [ForeignKey(nameof(TenantId))]
        public virtual ApplicationTenant Tenant { get; set; } = default!;
        private readonly List<DomainEvent> _domainEvents = new();

        [NotMapped]
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
