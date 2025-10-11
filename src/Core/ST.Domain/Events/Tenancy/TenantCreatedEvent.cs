using System;
using ST.Domain.Events.Common;

namespace ST.Domain.Tenancy
{
    public class TenantCreatedEvent : DomainEvent
    {
        public int TenantId { get; }

        public TenantCreatedEvent(int tenantId)
        {
            TenantId = tenantId;
        }
    }
}
