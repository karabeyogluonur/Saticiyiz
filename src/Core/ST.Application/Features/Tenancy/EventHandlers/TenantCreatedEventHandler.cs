using MediatR;
using ST.Domain.Tenancy;

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
