using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Identity;
using ST.Domain.Entities.Identity;
using ST.Domain.Events;
using ST.Domain.Events.Tenancy;

namespace ST.Application.Features.Tenancy.EventHandlers
{
    public class TenantCreatedEventHandler : INotificationHandler<TenantCreatedEvent>
    {
        public TenantCreatedEventHandler()
        {
        }

        public async Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
        {
            return;
        }
    }
}