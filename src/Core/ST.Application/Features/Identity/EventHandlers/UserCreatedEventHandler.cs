using MediatR;
using Microsoft.AspNetCore.Identity;
using ST.Application.Common.Constants;
using ST.Application.Interfaces.Identity;
using ST.Domain.Entities.Identity;
using ST.Domain.Events;
using ST.Domain.Events.Tenancy;
using ST.Domain.Identity;

namespace ST.Application.Features.Identity.EventHandlers
{
    public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
    {
        public UserCreatedEventHandler()
        {
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            return;
        }
    }
}