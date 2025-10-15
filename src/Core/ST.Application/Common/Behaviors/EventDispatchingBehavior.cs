using MediatR;
using Microsoft.Extensions.Logging;
using ST.Application.Interfaces.Repositories;

namespace ST.Application.Common.Behaviors
{
    /// <summary>
    /// Her Command/Query'den sonra çalışarak, UnitOfWork'te birikmiş
    /// Domain Event'leri IMediator aracılığıyla fırlatır.
    /// </summary>
    public class EventDispatchingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<EventDispatchingBehavior<TRequest, TResponse>> _logger;

        public EventDispatchingBehavior(IUnitOfWork unitOfWork, IMediator mediator, ILogger<EventDispatchingBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Önce CommandHandler'ın çalışmasına izin ver.
            var response = await next();

            // Handler çalıştıktan sonra, birikmiş event'leri al.
            var domainEvents = _unitOfWork.GetDomainEvents();

            // Eğer event yoksa, işlemi bitir.
            if (!domainEvents.Any())
            {
                return response;
            }

            // Event'leri fırlatmadan önce context'ten temizle.
            _unitOfWork.ClearDomainEvents();

            _logger.LogInformation("----- {EventCount} adet Domain Event fırlatılıyor: {RequestName} -----", domainEvents.Count, typeof(TRequest).Name);

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            return response;
        }
    }
}