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
   
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddInventoryRecord([FromBody] InventoryRequest request)
        {
            if (request == null || request.InvDetail == null || !request.InvDetail.Any())
            {
                return BadRequest(new { message = "Inventory list is empty or data is missing." });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID

                var inventory = new Inventory
                {
                    DateOfInventory = DateTime.Now,
                    UserId = userId,
                    Status = "Add Record"
                };

                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                foreach (var item in request.InvDetail)
                {
                    var inventoryDetails = new InventoryDetails
                    {
                        InventoryId = inventory.InventoryId,
                        ItemName = item.ItemName,
                        UnitOfMeasure = item.UnitofMeasure,
                        Quantity = item.Quantity
                    };

                    _context.InventoryDetails.Add(inventoryDetails);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Inventory record added successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while saving data.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetInventoryDetails(int id)
        {
            try
            {
                var inventoryDetails = await _context.InventoryDetails
                    .Where(d => d.InventoryId == id)
                    .Select(d => new
                    {
                        d.ItemName,
                        d.UnitOfMeasure,
                        d.Quantity
                    })
                    .ToListAsync();

                if (inventoryDetails == null || !inventoryDetails.Any())
                {
                    return NotFound(new { message = "No details found for this inventory record." });
                }

                return Ok(inventoryDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching inventory details.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        public class InventoryRequest
        {
            public string UserId { get; set; }
            public List<InventoryDetailItem> InvDetail { get; set; }
        }

        public class InventoryDetailItem
        {
            public string ItemName { get; set; }
            public string UnitofMeasure { get; set; }
            public int Quantity { get; set; }
        }
    }

}
