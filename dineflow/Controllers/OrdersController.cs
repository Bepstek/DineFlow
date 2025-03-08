using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dineflow.Models;
using dineflow.Data;
using System.Security.Claims;

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
            string newOrderId = await GenerateOrderId();
            // Step 1: Insert into Transactions_tb
            var transaction = new Transaction
            {
                ReservationId = request.ReservationId,
                OrderId = newOrderId,
                TableId = request.TableId,
                UserId = request.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = request.Cart.Sum(item => item.Price * item.Quantity),
                Status = request.Status
            };
            if(request.ReservationId != null)
            {
                updatereservation(request.ReservationId);
            }
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
        private async Task<string> GenerateOrderId()
        {
            string todayDate = DateTime.UtcNow.ToString("yyyyMMdd"); // Example: "20240303"
            string startingOrderId = "A000"; // Default first order of the day

            // Get the latest order for today
            var latestOrder = await _context.Transactions
                .Where(t => t.OrderDate == DateTime.UtcNow.Date)
                .OrderByDescending(t => t.OrderId)
                .FirstOrDefaultAsync();

            string newOrderId;

            if (latestOrder != null)
            {
                // Extract the last used Order ID
                string lastOrderId = latestOrder.OrderId;
                char letter = lastOrderId[0]; // Get the letter part (e.g., "A")
                int number = int.Parse(lastOrderId.Substring(1)); // Get the number part

                if (number < 999)
                {
                    number++; // Increment the number
                }
                else
                {
                    letter++; // Move to the next letter (e.g., A999 -> B001)
                    number = 1;
                }

                newOrderId = $"{letter}{number:D3}"; // Format as "A001", "B001", etc.
            }
            else
            {
                // First order of the day
                newOrderId = startingOrderId;
            }

            // Ensure the ID is unique by checking if it exists before returning it
            bool exists = await _context.Transactions.AnyAsync(t => t.OrderId == newOrderId);
            while (exists)
            {
                // If the generated ID already exists, increment the number again
                char letter = newOrderId[0];
                int number = int.Parse(newOrderId.Substring(1));

                if (number < 999)
                {
                    number++;
                }
                else
                {
                    letter++;
                    number = 1;
                }

                newOrderId = $"{letter}{number:D3}";

                exists = await _context.Transactions.AnyAsync(t => t.OrderId == newOrderId);
            }

            return newOrderId;
        }

        private void updatereservation(int id)
        {
            var reservation = _context.Reservations.Find(id);
            

            reservation.Status = "Complete";
            _context.Reservations.Update(reservation);
            _context.SaveChanges();
            
            
        }

        public class OrderRequest
        {
            public int ReservationId { get; set; }
           
            public int TableId { get; set; }
            public string UserId { get; set; }
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
