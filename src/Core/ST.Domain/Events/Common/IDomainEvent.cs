using ST.Domain.Events;
using System.Collections.Generic;

namespace ST.Domain.Events.Common
{
    public interface IDomainEvent
    {
        public IReadOnlyCollection<DomainEvent> DomainEvents { get; }
        public void AddDomainEvent(DomainEvent domainEvent);
        public void ClearDomainEvents();
    }
}