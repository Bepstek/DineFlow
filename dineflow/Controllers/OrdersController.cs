using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dineflow.Models;
using dineflow.Data;

namespace dineflow.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] OrderRequest request)
        {
            if (request == null || request.Cart == null || !request.Cart.Any())
            {
                return BadRequest("Cart is empty or data is missing.");
            }

            // Step 1: Insert into Transactions_tb
            var transaction = new Transaction
            {
                ReservationId = request.ReservationId,
                TableId = request.TableId,
                UserId = request.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = request.Cart.Sum(item => item.Price * item.Quantity),
                Status = request.Status
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(); // Save and generate transaction_id

            // Step 2: Insert into Transaction_details_tb using transaction_id
            foreach (var item in request.Cart)
            {
                var transactionDetail = new TransactionDetail
                {
                    TransactionId = transaction.TransactionId, // Use the generated ID
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Price * item.Quantity
                };

                _context.TransactionDetails.Add(transactionDetail);
            }

            await _context.SaveChangesAsync(); // Save all transaction details

            return Ok(new { message = "Order placed successfully!", transactionId = transaction.TransactionId });
        }

        
        public class OrderRequest
        {
            public int ReservationId { get; set; }
            public int TableId { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; }
            public List<CartItem> Cart { get; set; }


        }

        // Cart Item Model
        public class CartItem
        {
            public int MenuItemId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
        }
    }
}
