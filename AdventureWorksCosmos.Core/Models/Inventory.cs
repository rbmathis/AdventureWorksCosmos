using System;

namespace AdventureWorksCosmos.Core.Models.Inventory
{
    public class Inventory : DocumentBase
    {
        public int QuantityAvailable { get; set; }

        public int ProductId { get; set; }

        public void Handle(StockRequestMessage message)
        {
            Process(message, e =>
            {
                if (QuantityAvailable >= message.AmountRequested)
                {
                    QuantityAvailable -= e.AmountRequested;
                    Send(new StockRequestConfirmedMessage
                    {
                        Id = Guid.NewGuid(),
                        OrderFulfillmentId = e.OrderFulfillmentId,
                        ProductId = ProductId
                    });
                }
                else
                {
                    Send(new StockRequestDeniedMessage
                    {
                        Id = Guid.NewGuid(),
                        OrderFulfillmentId = e.OrderFulfillmentId,
                        ProductId = ProductId
                    });
                }
            });
        }

        public void Handle(StockReturnRequestedMessage message)  => Process(message, e => QuantityAvailable += e.AmountToReturn);
    }
}