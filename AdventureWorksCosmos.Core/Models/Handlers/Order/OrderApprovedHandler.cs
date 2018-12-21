using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using AdventureWorksCosmos.Core.Models.Orders;

namespace AdventureWorksCosmos.Core.Models.Fulfillments
{
    public class OrderApprovedHandler : IDocumentMessageHandler<OrderApprovedMessage>
    {
        private readonly IDocumentDBRepository<OrderSaga> _repository;

        public OrderApprovedHandler(IDocumentDBRepository<OrderSaga> repository) => _repository = repository;

        public async Task Handle(OrderApprovedMessage message)
        {
            var orderFulfillment = (await _repository.ListAsync(s => s.OrderId == message.OrderId)).FirstOrDefault();

            if (orderFulfillment == null)
            {
                orderFulfillment = new OrderSaga
                {
                    OrderId = message.OrderId
                };

                await _repository.CreateAsync(orderFulfillment);
            }

            orderFulfillment.Handle(message);

            await _repository.UpdateAsync(orderFulfillment);

        }
    }
}