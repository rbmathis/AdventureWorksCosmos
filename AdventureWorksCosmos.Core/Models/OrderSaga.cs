using System;
using System.Collections.Generic;
using System.Linq;
using AdventureWorksCosmos.Core.Models.Inventory;
using AdventureWorksCosmos.Core.Models.Orders;

namespace AdventureWorksCosmos.Core.Models.Fulfillments
{
    public class OrderSaga : DocumentBase
    {
        public Guid OrderId { get; set; }
        public bool IsCancelled { get; set; }
        public bool CancelOrderRequested { get; set; }
        public List<LineItem> LineItems { get; set; }
        public bool OrderApproved { get; set; }
        public bool OrderRejected { get; set; }

        public class LineItem
        {
            public int ProductId { get; set; }
            public int AmountRequested { get; set; }
            public bool StockConfirmed { get; set; }
            public bool StockReturnRequested { get; set; }
        }
        
        public void Handle(OrderCreatedMessage message)
        {
            Process(message, m =>
            {
                if (IsCancelled)
                    return;

                LineItems = m.LineItems.Select(li => new LineItem
                            {
                                ProductId = li.ProductId,
                                AmountRequested = li.Quantity
                            })
                    .ToList();

                foreach (var lineItem in LineItems)
                {
                    Send(new StockRequestMessage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = lineItem.ProductId,
                        AmountRequested = lineItem.AmountRequested,
                        OrderFulfillmentId = Id
                    });
                }
            });
        }

        public void Handle(OrderApprovedMessage message)
        {
            Process(message, m =>
            {
                OrderApproved = true;

                if (IsCancelled)
                {
                    ProcessCancellation();
                }
                else
                {
                    CheckForSuccess();
                }
            });
        }

        public void Handle(StockRequestConfirmedMessage message)
        {
            Process(message, m =>
            {
                var lineItem = LineItems.Single(li => li.ProductId == m.ProductId);
                lineItem.StockConfirmed = true;

                if (IsCancelled)
                {
                    ReturnStock(lineItem);
                }
                else
                {
                    CheckForSuccess();
                }
            });
        }

        public void Handle(StockRequestDeniedMessage message)
        {
            Process(message, m =>
            {
                Cancel();
            });
        }

        public void Handle(OrderRejectedMessage message)
        {
            Process(message, m =>
            {
                OrderRejected = true;

                Cancel();
            });
        }

        private void CheckForSuccess()
        {
            if (IsCancelled)
                return;

            if (LineItems.All(li => li.StockConfirmed) && OrderApproved)
            {
                Send(new OrderFulfillmentSuccessfulMessage
                {
                    Id = Guid.NewGuid(),
                    OrderId = OrderId
                });
            }
        }

        private void Cancel()
        {
            IsCancelled = true;

            ProcessCancellation();
        }

        private void ProcessCancellation()
        {
            if (!CancelOrderRequested && !OrderRejected)
            {
                CancelOrderRequested = true;
                Send(new CancelOrderRequestMessage
                {
                    Id = Guid.NewGuid(),
                    OrderId = OrderId
                });
            }

            foreach (var lineItem in LineItems.Where(li => li.StockConfirmed))
            {
                ReturnStock(lineItem);
            }
        }

        private void ReturnStock(LineItem lineItem)
        {
            if (lineItem.StockReturnRequested)
                return;

            lineItem.StockReturnRequested = true;
            Send(new StockReturnRequestedMessage
            {
                Id = Guid.NewGuid(),
                ProductId = lineItem.ProductId,
                AmountToReturn = lineItem.AmountRequested
            });
        }
    }
}