using System;
using ST.Domain.Entities.Identity;
using ST.Domain.Events.Common;

namespace ST.Domain.Identity
{
    public class UserCreatedEvent : DomainEvent
    {
        public ApplicationUser User { get; }

        public UserCreatedEvent(ApplicationUser user)
        {
            User = user;
        }
    }
}
