using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using AdventureWorksCosmos.Core.Models.Fulfillments;

namespace AdventureWorksCosmos.Core.Models.Orders
{
    public class OrderFulfillmentSuccessfulHandler : IDocumentMessageHandler<OrderFulfillmentSuccessfulMessage>
    {
        private readonly IDocumentDBRepository<Order> _repository;

        public OrderFulfillmentSuccessfulHandler(IDocumentDBRepository<Order> repository)
            => _repository = repository;

        public async Task Handle(OrderFulfillmentSuccessfulMessage message)
        {
            var order = await _repository.LoadAsync(message.OrderId);

            order.Handle(message);
        }
    }
}