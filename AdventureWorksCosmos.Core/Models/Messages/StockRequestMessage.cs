using System;

namespace AdventureWorksCosmos.Core.Models.Inventory
{
    public class StockRequestMessage : IDocumentMessage
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int AmountRequested { get; set; }
        public Guid OrderFulfillmentId { get; set; }
    }
}