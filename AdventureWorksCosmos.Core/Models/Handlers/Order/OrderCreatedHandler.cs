using System;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using AdventureWorksCosmos.Core.Models.Orders;
using NServiceBus.Logging;

namespace AdventureWorksCosmos.Core.Models.Fulfillments
{
    public class OrderCreatedHandler : IDocumentMessageHandler<OrderCreatedMessage>
    {
		static ILog log = LogManager.GetLogger<CancelOrderRequestHandler>();
		private readonly IDocumentDBRepository<OrderSaga> _repository;

        public OrderCreatedHandler(IDocumentDBRepository<OrderSaga> repository)  => _repository = repository;

        public async Task Handle(OrderCreatedMessage message)
        {
			log.Info($"Handling OrderCreatedMessage for : {message.OrderId}");

			var orderSaga = (await _repository.ListAsync(s => s.OrderId == message.OrderId)).FirstOrDefault();

            if (orderSaga == null)
            {
				log.Info($"Saga doesn't exist, so creating it");
				orderSaga = new OrderSaga
                {
                    Id = Guid.NewGuid(),
                    OrderId = message.OrderId
                };

                await _repository.CreateAsync(orderSaga);
            }

            orderSaga.Handle(message);

            await _repository.UpdateAsync(orderSaga);
        }
    }
}