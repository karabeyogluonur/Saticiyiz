using MediatR;
using ST.Application.Common.Attributes;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Events.Common;
using System.Reflection;

namespace ST.Application.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public TransactionBehavior(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            Type handlerType = next.Target.GetType();
            TransactionalAttribute transactionalAttribute = handlerType.GetCustomAttribute<TransactionalAttribute>();

            if (transactionalAttribute == null)
            {
                return await next();
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                TResponse response = await next();

                await _unitOfWork.CommitAsync();

                await DispatchEvents();

                return response;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task DispatchEvents()
        {
            IEnumerable<DomainEvent> events = _unitOfWork.GetDomainEvents();

            foreach (DomainEvent domainEvent in events)
            {
                await _mediator.Publish(domainEvent);
            }
            _unitOfWork.ClearDomainEvents();
        }
    }
}