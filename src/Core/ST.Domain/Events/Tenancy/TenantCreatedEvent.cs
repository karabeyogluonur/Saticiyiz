using System;
using ST.Domain.Entities;
using ST.Domain.Events.Common;

namespace ST.Domain.Tenancy
{
    public class TenantCreatedEvent : DomainEvent
    {
        public ApplicationTenant Tenant { get; }

        public TenantCreatedEvent(ApplicationTenant tenant)
        {
            Tenant = tenant;
        }
    }
}
