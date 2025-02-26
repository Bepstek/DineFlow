using System.Diagnostics;
using dineflow.Data;
using dineflow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace dineflow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Inject database context
        private readonly RoleManager<IdentityRole> _roleManager;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
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
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard"); 
            }
            return View();
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
            return View();
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
                    DateTime = DateTime.Now,
                };

                _context.Reservations.Add(newReservation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Booking");
            }

            return View("Booking", reservation); // Return view with validation messages if invalid
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
