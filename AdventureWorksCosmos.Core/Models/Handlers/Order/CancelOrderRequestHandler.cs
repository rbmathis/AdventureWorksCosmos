using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using NServiceBus.Logging;

namespace AdventureWorksCosmos.Core.Models.Orders
{
    public class CancelOrderRequestHandler : IDocumentMessageHandler<CancelOrderRequestMessage>
    {
		static ILog log = LogManager.GetLogger<CancelOrderRequestHandler>();

		private readonly IDocumentDBRepository<Order> _repository;

		public CancelOrderRequestHandler(IDocumentDBRepository<Order> repository)  => _repository = repository;

        public async Task Handle(CancelOrderRequestMessage message)
        {
			log.Info($"Handling CancelOrder for : {message.OrderId}");
			var order = await _repository.LoadAsync(message.OrderId);
			order.Handle(message);
        }
    }
}