using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using AdventureWorksCosmos.Core.Models.Orders;
using NServiceBus.Logging;

namespace AdventureWorksCosmos.Core.Models.Fulfillments
{
    public class OrderApprovedHandler : IDocumentMessageHandler<OrderApprovedMessage>
    {
		static ILog log = LogManager.GetLogger<CancelOrderRequestHandler>();
		private readonly IDocumentDBRepository<OrderSaga> _repository;

        public OrderApprovedHandler(IDocumentDBRepository<OrderSaga> repository) => _repository = repository;

        public async Task Handle(OrderApprovedMessage message)
        {
			log.Info($"Handling OrderApprovedMessage for : {message.OrderId}");

			var orderSaga = (await _repository.ListAsync(s => s.OrderId == message.OrderId)).FirstOrDefault();

            if (orderSaga == null)
            {
                orderSaga = new OrderSaga
                {
                    OrderId = message.OrderId
                };

                await _repository.CreateAsync(orderSaga);
            }

            orderSaga.Handle(message);

            await _repository.UpdateAsync(orderSaga);

        }
    }
}