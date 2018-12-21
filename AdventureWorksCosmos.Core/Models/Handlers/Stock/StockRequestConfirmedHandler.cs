using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using AdventureWorksCosmos.Core.Models.Inventory;

namespace AdventureWorksCosmos.Core.Models.Fulfillments
{
    public class StockRequestConfirmedHandler : IDocumentMessageHandler<StockRequestConfirmedMessage>
    {
        private readonly IDocumentDBRepository<OrderSaga> _repository;

        public StockRequestConfirmedHandler(IDocumentDBRepository<OrderSaga> repository) => _repository = repository;

        public async Task Handle(StockRequestConfirmedMessage message)
        {
            var orderFulfillment = await _repository.LoadAsync(message.OrderFulfillmentId);

            orderFulfillment.Handle(message);

            await _repository.UpdateAsync(orderFulfillment);

        }
    }
}