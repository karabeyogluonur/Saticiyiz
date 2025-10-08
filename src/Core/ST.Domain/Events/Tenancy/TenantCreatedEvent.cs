using System;
using ST.Domain.Events.Common;

namespace ST.Domain.Events.Tenancy
{
    public class TenantCreatedEvent : DomainEvent
    {
        public string TenantId { get; }
        public string TenantName { get; }

        public TenantCreatedEvent(string tenantId, string tenantName)
        {
            TenantId = tenantId;
            TenantName = tenantName;
        }
    }
}