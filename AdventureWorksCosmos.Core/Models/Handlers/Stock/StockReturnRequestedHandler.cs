﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;

namespace AdventureWorksCosmos.Core.Models.Inventory
{
    public class StockReturnRequestedHandler : IDocumentMessageHandler<StockReturnRequestedMessage>
    {
        private readonly IDocumentDBRepository<Inventory> _repository;

        public StockReturnRequestedHandler(IDocumentDBRepository<Inventory> repository)
            => _repository = repository;

        public async Task Handle(StockReturnRequestedMessage message)
        {
            var stock = (await _repository
                    .ListAsync(s => s.ProductId == message.ProductId))
                .FirstOrDefault();

            if (stock == null)
            {
                stock = new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = message.ProductId,
                    QuantityAvailable = 100
                };

                await _repository.CreateAsync(stock);
            }

            stock.Handle(message);

            await _repository.UpdateAsync(stock);
        }
    }
}