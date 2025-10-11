using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using ST.Domain.Entities.Common;
using ST.Domain.Events.Common;

namespace ST.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser<int>, IDomainEvent, ITenantEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public virtual ApplicationTenant Tenant { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HasAcceptedTerms { get; set; } = true;
        public DateTime? TermsAcceptedAt { get; set; } = DateTime.UtcNow;

        public bool IsSubscribedToNewsletter { get; set; } = true;
        public DateTime? NewsletterSubscribedAt { get; set; } = DateTime.UtcNow;

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
