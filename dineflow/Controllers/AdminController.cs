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
    [Authorize(Roles ="Admin")]
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
        public async Task<IActionResult> Menu(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.Menus.Include(m => m.Category).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.Category.Name.Contains(search));
            }

            // Pagination
            int totalRecords = await query.CountAsync();
            var menuItems = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.SearchQuery = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return View(menuItems);
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

        public IActionResult EditDish(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null)
            {
                return NotFound();
            }

            // Populate the dropdown with category data
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", menu.CategoryId);

            return View(menu);
        }




        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDish(int id, IFormCollection collection)
        {
            try
            {
                var existingDish = await _context.Menus.FindAsync(id);

                if (existingDish != null)
                {
                    existingDish.Name = collection["Name"];
                    existingDish.CategoryId = int.Parse(collection["CategoryId"]);
                    existingDish.Description = collection["Description"];
                    existingDish.Price = decimal.Parse(collection["Price"]);
                   

                    // Check if an image was uploaded
                    var file = Request.Form.Files["ImageBase64"];
                    if (file != null && file.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            existingDish.ImageBase64 = Convert.ToBase64String(memoryStream.ToArray());
                        }
                    }

                    _context.Menus.Update(existingDish);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

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
                await _context.Categories.AddAsync(new Category { Name = category.Name });
                await _context.SaveChangesAsync();
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

                if (existingCategory != null)
                {
                    existingCategory.Name = collection["Name"]; // Get updated name from form
                    _context.Categories.Update(existingCategory);
                    await _context.SaveChangesAsync();
                }

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

            return RedirectToAction("Management");
        }


    }
}
