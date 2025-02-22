using dineflow.Controllers;
using dineflow.Data;
using dineflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize] // Ensure only authenticated users can access
public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly ApplicationDbContext _context; // Inject database context
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DashboardController(ILogger<DashboardController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Index()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public IActionResult Inventory()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public IActionResult Management()
    {
        var users = _userManager.Users.ToList(); // Get all users
        return View(users);
    }
    public IActionResult Pos()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public async Task<IActionResult> Reservation()
        {
            var bookings = await _context.Bookings.ToListAsync();
            return View(bookings); 
        }
    public IActionResult Menu()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public IActionResult Settings()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public IActionResult Table()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public IActionResult Transaction()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
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
    public IActionResult AddMenu()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> CreateMenu(Menu menu, IFormFile ImageFile)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await ImageFile.CopyToAsync(ms); // Copy image to memory stream
                        byte[] imageBytes = ms.ToArray(); // Convert to byte array
                        menu.ImageBase64 = Convert.ToBase64String(imageBytes); // Store as Base64 string
                    }
                }

                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Menu added successfully!";
                return RedirectToAction("Menu");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting menu: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the menu.");
            }
        }

        return View("AddMenu", menu);
    }


}
