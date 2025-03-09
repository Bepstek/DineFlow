using System.Diagnostics;
using dineflow.Data;
using dineflow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using dineflow.Services;
using dineflow.ViewModel;
using Microsoft.EntityFrameworkCore;
namespace dineflow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Inject database context
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, RoleManager<IdentityRole> roleManager,EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
            _emailService = emailService;
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
        public IActionResult Index()
        {
//////SAMPLEE
            var reservation = _context.Reservations.ToList();
            // Fetch menu items where IsArchived is false and the related category is not archived
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

            // Pass categories to the ViewBag
            ViewBag.Categories = categories;
            ViewBag.Reservation = reservation;
            return View(menuItems);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Service()
        {
            return View();
        }

        public IActionResult Menu()
        {
            // Fetch menu items where IsArchived is false and the related category is not archived
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

            // Pass categories to the ViewBag
            ViewBag.Categories = categories;

            return View(menuItems);
        }






        public IActionResult Booking()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBooking(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                var newReservation = new Reservation
                {
                    Name = reservation.Name,
                    Email = reservation.Email,
                    PhoneNumber = reservation.PhoneNumber,
                    NumberOfPeople = reservation.NumberOfPeople,
                    ReservedDate = reservation.ReservedDate,
                    Status = reservation.Status,
                    DateTime = DateTime.Now
                   
                };

                _context.Reservations.Add(newReservation);
                await _context.SaveChangesAsync();

                // Send Confirmation Email
                string subject = "DineFlow Reservation Confirmation";
                string message = $@"
                Dear {reservation.Name},<br><br>
                Thank you for your reservation at <b>DineFlow</b>!<br><br>
                <b>Reservation Details:</b><br>
                - Name: {reservation.Name}<br>
                - Email: {reservation.Email}<br>
                - Phone: {reservation.PhoneNumber}<br>
                - Date & Time: {reservation.ReservedDate}<br>
                - Number of People: {reservation.NumberOfPeople}<br><br>
                We look forward to serving you!<br><br>
                Regards,<br>
                <b>DineFlow Team</b>
            ";

                await _emailService.SendEmailAsync(reservation.Email, subject, message);

                return RedirectToAction("Booking");
            }

            return View("Booking", reservation);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
