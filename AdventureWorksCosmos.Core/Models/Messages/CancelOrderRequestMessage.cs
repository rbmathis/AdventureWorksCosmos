using System;

namespace AdventureWorksCosmos.Core.Models.Orders
{
    public class CancelOrderRequestMessage : IDocumentMessage
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
    }
}