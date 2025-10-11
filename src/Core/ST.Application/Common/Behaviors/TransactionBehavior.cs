using MediatR;
using Microsoft.Extensions.Logging;
using ST.Application.Common.Attributes;
using ST.Application.Interfaces.Repositories;
using ST.Domain.Events.Common;
using System.Reflection;
using System.Transactions;

namespace ST.Application.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        public TransactionBehavior(IUnitOfWork unitOfWork, IMediator mediator, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var transactionalAttribute = request.GetType().GetCustomAttribute<TransactionalAttribute>();
            if (transactionalAttribute is null)
            {
                return await next();
            }

            _logger.LogInformation("----- Başlatılıyor: Transactional Operation for {RequestName} -----", typeof(TRequest).Name);

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            TResponse response;
            try
            {
                response = await next();

                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "----- HATA: Transactional Operation for {RequestName} geri alınıyor. -----", typeof(TRequest).Name);
                throw;
            }

            _logger.LogInformation("----- Tamamlandı: Transactional Operation for {RequestName} -----", typeof(TRequest).Name);

            var domainEvents = _unitOfWork.GetDomainEvents();
            _unitOfWork.ClearDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                _logger.LogInformation("Dispatching domain event: {EventName}", domainEvent.GetType().Name);
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            return response;
        }
    }
}
