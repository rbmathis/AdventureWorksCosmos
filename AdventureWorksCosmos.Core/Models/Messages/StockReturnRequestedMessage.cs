using System;

namespace AdventureWorksCosmos.Core.Models.Inventory
{
    public class StockReturnRequestedMessage : IDocumentMessage
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int AmountToReturn { get; set; }
    }
}