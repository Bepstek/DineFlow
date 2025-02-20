using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize] // Ensure only authenticated users can access
public class DashboardController : Controller
{
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
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public IActionResult Pos()
    {
        return View(); // No need to check authentication, the [Authorize] attribute already ensures it
    }
    public IActionResult Product()
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
}
