using MediatR;

namespace ST.Domain.Events.Common
{
    public abstract class DomainEvent : INotification
    {
        protected DomainEvent()
        {
            DateOccurred = DateTime.UtcNow;
        }

        public DateTime DateOccurred { get; protected set; }
    }
}