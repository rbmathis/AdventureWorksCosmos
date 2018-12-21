using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureWorksCosmos.Core.Infrastructure;
using AdventureWorksCosmos.Core.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdventureWorksCosmos.UI.Pages.Orders
{
    public class ShowModel : PageModel
    {
        private readonly IDocumentDBRepository<Order> _db;

        public ShowModel(IDocumentDBRepository<Order> db) => _db = db;

        public async Task OnGet(Guid id)
        {
            Order = await _db.LoadAsync(id);
        }

        public Order Order { get; set; }
    }
}