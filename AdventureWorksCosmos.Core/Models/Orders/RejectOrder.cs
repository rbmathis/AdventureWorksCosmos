using System;
using System.Threading;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using MediatR;

namespace AdventureWorksCosmos.Core.Models.Orders
{
    public class RejectOrder
    {
        public class Request : IRequest
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Request>
        {
            private readonly IDocumentDBRepository<Order> _orderRepository;

            public Handler(IDocumentDBRepository<Order> orderRepository)
            {
                _orderRepository = orderRepository;
            }

            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderRequest = await _orderRepository.LoadAsync(request.Id);

                orderRequest.Reject();
                
                await _orderRepository.UpdateAsync(orderRequest);

                return Unit.Value;
            }
        }
    }
}