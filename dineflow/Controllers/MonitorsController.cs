using dineflow.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dineflow.Controllers
{
    public class MonitorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MonitorsController(ApplicationDbContext context)
        {
            _context = context;
        }
        //views for kitchen monitor
        public IActionResult Kitchen()
        {
            var transactions = _context.Transactions
        .Include(t => t.TransactionDetails)
        .ThenInclude(td => td.MenuItem) // Ensure MenuItem is loaded
        .ToList();

            return View(transactions);

        }
        public IActionResult ServingOrder(int id)
        {
            var transaction = _context.Transactions.Find(id);
            if (transaction == null)
            {
                return NotFound();
            }

            transaction.Status = "Serving";
            _context.Transactions.Update(transaction);
            _context.SaveChanges();

            return RedirectToAction("Kitchen");
        }
        //views for ordermonitor
        public IActionResult Orders()
        {
            var transactions = _context.Transactions
        .Include(t => t.TransactionDetails)
        .ThenInclude(td => td.MenuItem) 
        .ToList();

            return View(transactions);

        }
        public IActionResult FetchOrders()
        {
            var transactions = _context.Transactions
                .Include(t => t.TransactionDetails)
                .ThenInclude(td => td.MenuItem)
                .Select(t => new
                {
                    TransactionId = t.TransactionId,
                    Status = t.Status
                })
                .ToList();

            return Json(transactions);
        }

    }
}
