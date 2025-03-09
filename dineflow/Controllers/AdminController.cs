using System.Security.Claims;
using dineflow.Data;
using dineflow.Models;
using dineflow.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;

namespace dineflow.Controllers
{
    [Authorize(Roles ="Admin,Manager")]
    public class AdminController : Controller
    {

        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(UserManager<IdentityUser> userManager, ILogger<AdminController> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
        }
       
        public IActionResult Management()
        {
            var users = _userManager.Users.OfType<ApplicationUser>().ToList(); // Ensure type matching
            return View(users);
        }
        public async Task<IActionResult> UsersDetail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }


        public IActionResult Table()
        {
            return View(); 
        }
        public async Task<IActionResult> Menu(string search, int page = 1, int pageSize = 5)
        {
            var query = _context.Menus.Include(m => m.Category).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.Category.Name.Contains(search));
            }

            // Apply category filter: Exclude menus from categories where Id is 1 or IsArchived is false
            query = query.Where(m => m.Category.Id != 1 && !m.Category.IsArchived);

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


        public IActionResult CreateDish()
        {
            var categories = _context.Categories
                                     .Where(c => !c.IsArchived) // Exclude archived categories
                                     .ToList();

            var viewModel = new MenuViewModel
            {
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateDish(MenuViewModel vm)
        {
            byte[] imageBytes = ConvertImageToByteArray(vm);

            var dish = new Menu
            {
                Name = vm.Name,
                CategoryId = vm.CategoryId,
                Description = vm.Description,
                Price = vm.Price,
                ImageBytes = imageBytes // Store the byte array in the database
            };

            _context.Menus.Add(dish);
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Create Dish", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Create dish: {vm.Name}");
            _context.SaveChanges();
            return RedirectToAction("Menu");
        }
        private byte[] ConvertImageToByteArray(MenuViewModel vm)
        {
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    vm.ImageFile.CopyTo(memoryStream);
                    return memoryStream.ToArray(); // Convert the image to a byte array
                }
            }
            return null; // Return null if no image is uploaded
        }


      

        public IActionResult GetDishById(int id)
        {
            var dish = _context.Menus
                .Include(m => m.Category)
                .FirstOrDefault(m => m.Id == id);

            if (dish == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = dish.Id,
                name = dish.Name,
                description = dish.Description,
                price = dish.Price,
                category = new { id = dish.Category.Id, name = dish.Category.Name },
                categories = _context.Categories
                    .Select(c => new { id = c.Id, name = c.Name })
                    .ToList(),
                imageUrl = dish.ImageBytes != null
                    ? Convert.ToBase64String(dish.ImageBytes)
                    : null // Convert byte array to Base64
            };

            return Json(result);
        }
        //to be removed
        public IActionResult EditDish(int id)
            {
                var menu = _context.Menus.Find(id);
                if (menu == null)
                {
                    return NotFound();
                }

                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", menu.CategoryId);

                return View(menu);
            }

        public IActionResult UpdateDish(Menu model, IFormFile imageFile)
        {
            // Validate the model
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid dish data." });
            }

            // Find the dish in the database
            var dish = _context.Menus.Find(model.Id);
            if (dish == null)
            {
                return Json(new { success = false, message = "Dish not found." });
            }

            // Update dish properties
            dish.Name = model.Name;
            dish.Description = model.Description;
            dish.Price = model.Price;
            dish.CategoryId = model.CategoryId;

            // Update the image if a new file is uploaded
            if (imageFile != null && imageFile.Length > 0)
            {
                byte[] imageBytes = ConvertImageToByteArray(imageFile);
                dish.ImageBytes = imageBytes; // Update the image bytes
            }

            try
            {
                // Log the activity
                var logsController = new LogsController(_context);
                logsController.ActivityLog(
                    "Update Dish",
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    $"Updated dish: {model.Name}" + (imageFile != null ? " (Image updated)" : "")
                );

                // Save changes to the database
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle errors
                return Json(new { success = false, message = "Error saving to database.", error = ex.Message });
            }
        }

        // Helper method to convert IFormFile to byte array
        private byte[] ConvertImageToByteArray(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                imageFile.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditDish(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        var existingDish = await _context.Menus.FindAsync(id);

        //        if (existingDish != null)
        //        {
        //            existingDish.Name = collection["Name"];
        //            existingDish.CategoryId = int.Parse(collection["CategoryId"]);
        //            existingDish.Description = collection["Description"];
        //            existingDish.Price = decimal.Parse(collection["Price"]);


        //            // Check if an image was uploaded
        //            var file = Request.Form.Files["ImageBase64"];
        //            if (file != null && file.Length > 0)
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    await file.CopyToAsync(memoryStream);
        //                    existingDish.ImageBase64 = Convert.ToBase64String(memoryStream.ToArray());
        //                }
        //            }
        //            var logsController = new LogsController(_context);
        //            logsController.ActivityLog("Update Dish", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Updated dish: {collection["Name"]}");
        //            _context.Menus.Update(existingDish);
        //            await _context.SaveChangesAsync();
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
        // remove edit method
        public IActionResult DetailDish(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                return NotFound();
            }
            return View(menu);
        }

        public IActionResult ArchiveDish(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                return NotFound();
            }

            menu.IsArchived = true;
            _context.Menus.Update(menu);
            _context.SaveChanges();
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Archive Dish", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Archive Dish: {menu.Name}");
            return RedirectToAction("Menu");
        }

        public IActionResult UnarchiveDish(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                return NotFound();
            }

            menu.IsArchived = false;
            _context.Menus.Update(menu);
            _context.SaveChanges();
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Unarchive Dish", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Unarchive Dish: {menu.Name}");
            return RedirectToAction("Menu");
        }



        public IActionResult Settings()
        {
            return View(); // No need to check authentication, the [Authorize] attribute already ensures it
        }
        public IActionResult Roles()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }
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
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Create Role", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Create Role: {role.Name}");
            return RedirectToAction("Index");
        }
        public IActionResult Categories(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsArchived = c.IsArchived,
                    DishCount = _context.Menus.Count(m => m.CategoryId == c.Id && !m.IsArchived) // Count only active dishes
                });

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            // Pagination
            int totalRecords = query.Count();
            var categories = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchQuery = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return View(categories);
        }



        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]

            public async Task<IActionResult> CreateCategory(Category category)
            {
                var existingCategory = await _context.Categories
                    .AnyAsync(c => c.Name == category.Name); // Check if category exists

                if (!existingCategory)
                {
                    // Add the new category
                    await _context.Categories.AddAsync(new Category { Name = category.Name });
                    await _context.SaveChangesAsync();

                    // Log the activity
                    var logsController = new LogsController(_context);
                    logsController.ActivityLog("Create Category", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Created category: {category.Name}");
                }

                return RedirectToAction("Categories");
            }
        


        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, IFormCollection collection)
        {
            try
            {
                var existingCategory = await _context.Categories.FindAsync(id);
                string name = existingCategory.Name;
                if (existingCategory != null)
                {
                    
                    existingCategory.Name = collection["Name"]; // Get updated name from form
                    _context.Categories.Update(existingCategory);
                    await _context.SaveChangesAsync();
                }
                var logsController = new LogsController(_context);
                logsController.ActivityLog("Update Category", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Update category: {name} To {collection["Name"]}");
                return RedirectToAction(nameof(Categories)); // Redirect to categories list
            }
            catch
            {
                return View();
            }
        }

        public IActionResult ArchiveCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsArchived = true;
            _context.Categories.Update(category);
            _context.SaveChanges();
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Archive Category", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Archive category: {category.Name}");
            return RedirectToAction("Categories");
        }

        public IActionResult UnarchiveCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsArchived = false;
            _context.Categories.Update(category);
            _context.SaveChanges();
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Unarchive Category", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Unarchive category: {category.Name}");
            return RedirectToAction("Categories");
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            _context.SaveChanges();
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Delete Category", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Delete category: {category.Name}");
            return RedirectToAction("Categories");
        }
        [HttpPost]
        public async Task<IActionResult> ArchiveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id) as ApplicationUser;
            if (user == null)
            {
                return NotFound();
            }

           
            user.LockoutEnd = DateTime.UtcNow.AddYears(100);

            // ✅ Update Status to "Deactivated"
            user.Status = "Deactivated";
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Deactivate User", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Deactivate User: {user.Lastname}");
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Management");
        }


        [HttpPost]
        public async Task<IActionResult> UnarchiveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id) as ApplicationUser;
            if (user == null)
            {
                return NotFound();
            }

            
            user.LockoutEnd = null;
            user.Status = "Active";
            await _userManager.UpdateAsync(user);
            var logsController = new LogsController(_context);
            logsController.ActivityLog("Activate User", User.FindFirstValue(ClaimTypes.NameIdentifier), $"Activate User: {user.Lastname}");
            return RedirectToAction("Management");
        }
        public async Task<IActionResult> Logs(string searchString, int pageNumber = 1, int pageSize = 10)
        {
            // Base query
            var query = _context.ActivityLogs
                .OrderByDescending(l => l.Timestamp) // Order by most recent
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(l =>
                    l.Action.Contains(searchString) ||
                    l.Details.Contains(searchString) ||
                    l.UserId.Contains(searchString));
            }

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var logs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new ActivityLog
                {
                    LogId = l.LogId,
                    UserId = l.UserId,
                    Action = l.Action,
                    Details = l.Details,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            // Pass data to the view
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(logs);
        }


    }
}
