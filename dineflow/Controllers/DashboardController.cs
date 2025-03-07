    using System.Net;
using dineflow.Controllers;
using dineflow.Data;
using System.Security.Claims;

    using dineflow.Models;
    using dineflow.ViewModel;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using static dineflow.Controllers.OrdersController;
using System.Globalization;

[Authorize] // Ensure only authenticated users can access
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DashboardController(ILogger<DashboardController> logger, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Role()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }
    [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(IdentityRole role)
        {
            if (!_roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
            }
            return RedirectToAction("Index");
        }
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var currentMonth = today.Month;
        var currentYear = today.Year;

        // Fetch data from the database
        var dashboardData = new DashboardViewModel
        {
            TotalMenu = await _context.Menus.CountAsync(),
            TodaySales = await _context.Transactions
                .Where(t => t.OrderDate.Date == today)
                .SumAsync(t => (decimal?)t.TotalAmount) ?? 0,
            TotalRevenue = await _context.Transactions
                .SumAsync(t => (decimal?)t.TotalAmount) ?? 0,
            TotalTransactions = await _context.Transactions.CountAsync(),
            OrderSummary = await GetTopSellingDishesAsync(5), // Fetch top 5 most selling dishes
            RevenueChartLabels = await GetRevenueChartLabelsAsync(), // Fetch labels for the revenue chart
            RevenueChartData = await GetRevenueChartDataAsync() // Fetch data for the revenue chart
        };

        return View(dashboardData);
    }

    private async Task<List<OrderSummaryItem>> GetTopSellingDishesAsync(int topCount)
    {
        var topDishes = await _context.TransactionDetails
            .GroupBy(oi => oi.MenuItem.Name)
            .Select(g => new
            {
                Category = g.Key,
                TotalSales = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(g => g.TotalSales)
            .Take(topCount)
            .ToListAsync();

        var totalSales = topDishes.Sum(d => d.TotalSales);

        return topDishes.Select(d => new OrderSummaryItem
        {
            Category = d.Category,
            Percentage = totalSales > 0 ? Math.Round((d.TotalSales / (decimal)totalSales) * 100, 2) : 0
        }).ToList();
    }

    // Helper method to get revenue chart labels (e.g., months)
    private async Task<List<string>> GetRevenueChartLabelsAsync()
    {
        return await _context.Transactions
            .GroupBy(t => t.OrderDate.Month)
            .OrderBy(g => g.Key)
            .Select(g => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key))
            .ToListAsync();
    }

    // Helper method to get revenue chart data (e.g., monthly revenue)
    private async Task<List<decimal>> GetRevenueChartDataAsync()
    {
        return await _context.Transactions
            .GroupBy(t => t.OrderDate.Month)
            .OrderBy(g => g.Key)
            .Select(g => g.Sum(t => t.TotalAmount))
            .ToListAsync();
    }
    public IActionResult Inventory(string search, int page = 1, int pageSize = 10)
    {
        var query = _context.Inventories
            .Include(i => i.User)
            .OrderByDescending(t => t.DateOfInventory)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => 
            i.User.Lastname.Contains(search) ||
            i.DateOfInventory.ToString().Contains(search) ||
            i.InventoryId.ToString().Contains(search)
            );
        }

        int totalRecords = query.Count();
        var inventoryList = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        ViewBag.SearchQuery = search; // Ensure this matches the input field's name

        return View(inventoryList);
    }





    public IActionResult Pos(int reservationId)
    {
        ViewBag.ReservationId = reservationId;

        // Fetch menu items where IsArchive is false and the related category is not archived
        var menuItems = _context.Menus
            .Include(m => m.Category)
            .Where(m => !m.IsArchived && m.Category != null && !m.Category.IsArchived)
            .ToList();

        // Fetch unique categories where IsArchive is false
        var categories = _context.Categories
            .Where(c => !c.IsArchived)
            .Select(c => c.Name)
            .Distinct()
            .ToList();

        // Pass both menu items and categories to the view
        ViewBag.Categories = categories;

        return View(menuItems);
    }



    public async Task<IActionResult> Reservation(string searchString, int pageNumber = 1, int pageSize = 10)
    {
        var bookings = _context.Reservations.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            bookings = bookings.Where(r => r.Name.Contains(searchString) || r.Email.Contains(searchString) || r.PhoneNumber.Contains(searchString));
        }

        // Pagination
        int totalItems = await bookings.CountAsync();
        var reservations = await bookings
            .OrderByDescending(r => r.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SearchString = searchString;
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalItems = totalItems;

        return View(reservations);
    }





    [HttpPost]
    public IActionResult CreateDish(MenuViewModel vm)
    {
        string stringFileName = UploadFile(vm);
        var dish = new Menu
        {
            Name = vm.Name,
            CategoryId = vm.CategoryId, // Ensure ViewModel has CategoryId
            Description = vm.Description,
            Price = vm.Price,
            ImageBase64 = stringFileName
        };

        _context.Menus.Add(dish);
        _context.SaveChanges();
        return RedirectToAction("Menu");
    }


    private string UploadFile(MenuViewModel vm)
    {
        string fileName = null;
        if (vm.ImageFile != null)
        {
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "dishimage");
            fileName = Guid.NewGuid().ToString() + "-" + vm.ImageFile.FileName;
            string filePath = Path.Combine(uploadDir, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                vm.ImageFile.CopyTo(fileStream);
            }
        }
        return fileName;
    }


    public async Task<IActionResult> Menu(string search, int page = 1, int pageSize = 3)
    {
        var query = _context.Menus.Include(m => m.Category).AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Name.Contains(search) || m.Category.Name.Contains(search));
        }

        // Populate ViewBag.Categories
        ViewBag.Categories = _context.Categories
                                     .Where(c => !c.IsArchived) // Exclude archived categories
                                     .Select(c => new { c.Id, c.Name })
                                     .ToList();

        // Pagination
        int totalItems = await query.CountAsync(); // Total items count
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        // Apply pagination
        var paginatedMenu = await query
            .OrderByDescending(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SearchString = search;
        ViewBag.PageNumber = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalItems = totalItems;
        ViewBag.TotalPages = totalPages;

        return View(paginatedMenu);
    }


    public async Task<IActionResult> TransactionDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var transactionDetails = await _context.TransactionDetails
            .Include(td => td.Transaction)
            .Include(td => td.MenuItem) // Include the related MenuItem table
            .Where(td => td.TransactionId == id)
            .ToListAsync();

        if (!transactionDetails.Any())
        {
            return NotFound();
        }

        return View(transactionDetails);
    }
[HttpGet]
public async Task<IActionResult> GetTransactionDetails(int id)
{
    try
    {
        var transaction = await _context.Transactions
            .Include(t => t.TransactionDetails) // Include transaction details
            .ThenInclude(td => td.MenuItem) // Include menu item details
            .FirstOrDefaultAsync(t => t.TransactionId == id);

        if (transaction == null)
        {
            return NotFound(new { message = "Transaction not found." });
        }

        var transactionDetails = new
        {
            TransactionId = transaction.TransactionId,
            Items = transaction.TransactionDetails.Select(td => new
            {
                ItemName = td.MenuItem?.Name,
                Quantity = td.Quantity,
                Price = td.Price.ToString("C"),
                Total = td.Total.ToString("C")
            }).ToList()
        };

        // Log the data being returned
        Console.WriteLine("Transaction Details:", transactionDetails);

        return Ok(transactionDetails);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            message = "An error occurred while fetching transaction details.",
            error = ex.Message,
            stackTrace = ex.StackTrace,
            innerException = ex.InnerException?.Message
        });
    }
}

    public IActionResult CompleteTransaction(int id)
    {
        var transaction = _context.Transactions.Find(id);
        if (transaction == null)
        {
            return NotFound();
        }

        transaction.Status = "Complete";
        _context.Transactions.Update(transaction);
        _context.SaveChanges();

        return RedirectToAction("Transaction");
    }

    [HttpGet]
    public async Task<IActionResult> GetFiltered(string date, string status, string searchId)
    {
        var transactions = _context.Transactions.AsNoTracking().AsQueryable(); // Prevents caching

        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
        {
            transactions = transactions.Where(t => t.OrderDate.Date == parsedDate.Date);
        }

        if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
        {
            transactions = transactions.Where(t => t.Status.ToLower() == status.ToLower());
        }

        if (!string.IsNullOrEmpty(searchId))
        {
            transactions = transactions.Where(t => t.TransactionId.ToString().Contains(searchId));
        }

        var result = await transactions
            .OrderByDescending(t => t.OrderDate) // Ensures newest transactions show first
            .Select(t => new
            {
                t.TransactionId,
                t.UserId,
                t.OrderDate,
                t.OrderId,
                t.TotalAmount,
                t.Status
            })
            .ToListAsync();

        return Json(result);
    }

    public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
    


}
