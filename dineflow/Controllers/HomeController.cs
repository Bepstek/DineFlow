    using System.Diagnostics;
    using dineflow.Models;
    using Microsoft.AspNetCore.Mvc;

    namespace dineflow.Controllers
    {
        public class HomeController : Controller
        {
            private readonly ILogger<HomeController> _logger;

            public HomeController(ILogger<HomeController> logger)
            {
                _logger = logger;
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
