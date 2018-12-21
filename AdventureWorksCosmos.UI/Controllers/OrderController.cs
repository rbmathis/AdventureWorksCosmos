using System;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Models.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorksCosmos.UI.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _mediator.Send(new ApproveOrder.Request { Id = id });

            return RedirectToPage("/Orders/Show", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            await _mediator.Send(new RejectOrder.Request { Id = id });

            return RedirectToPage("/Orders/Show", new { id });
        }
    }
}