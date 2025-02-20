using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize] // Ensure only authenticated users can access
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
}
