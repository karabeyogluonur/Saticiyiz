using MediatR;
using Microsoft.Extensions.Logging;
using ST.Application.Common.Attributes;
using System.Reflection;
using System.Transactions;

namespace ST.Application.Common.Behaviors
{
    /// <summary>
    /// SADECE transaction yönetiminden sorumludur.
    /// </summary>
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var transactionalAttribute = request.GetType().GetCustomAttribute<TransactionalAttribute>();
            if (transactionalAttribute is null)
            {
                // Attribute yoksa, bu behavior'ın bir işi yok. Sadece bir sonraki adımı çağır.
                return await next();
            }

            _logger.LogInformation("----- Transaction Başlatılıyor: {RequestName} -----", typeof(TRequest).Name);

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            TResponse response;
            try
            {
                // Bir sonraki adıma geç (bu, EventDispatchingBehavior'ı ve ardından CommandHandler'ı çalıştıracak).
                response = await next();

                // Her şey başarılı olursa, transaction'ı onaya hazırla.
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "----- HATA: Transaction Geri Alınıyor: {RequestName} -----", typeof(TRequest).Name);
                throw;
            }

            _logger.LogInformation("----- Transaction Tamamlandı: {RequestName} -----", typeof(TRequest).Name);

            return response;
        }
    }
}