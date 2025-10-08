using System;
using ST.Domain.Events.Common;

namespace ST.Domain.Identity
{
    public class UserCreatedEvent : DomainEvent
    {
        public int UserId { get; }

        public UserCreatedEvent(int userId)
        {
            UserId = userId;
        }
    }
}