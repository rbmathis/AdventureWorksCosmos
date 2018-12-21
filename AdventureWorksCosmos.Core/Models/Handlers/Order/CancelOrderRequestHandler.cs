﻿using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;

namespace AdventureWorksCosmos.Core.Models.Orders
{
    public class CancelOrderRequestHandler : IDocumentMessageHandler<CancelOrderRequestMessage>
    {
        private readonly IDocumentDBRepository<Order> _repository;

        public CancelOrderRequestHandler(IDocumentDBRepository<Order> repository) 
            => _repository = repository;

        public async Task Handle(CancelOrderRequestMessage message)
        {
            var order = await _repository.LoadAsync(message.OrderId);

            order.Handle(message);
        }
    }
}