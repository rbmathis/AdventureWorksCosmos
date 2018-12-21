using System;

namespace AdventureWorksCosmos.Core.Models.Orders
{
    public class OrderRejectedMessage : IDocumentMessage
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
    }
}