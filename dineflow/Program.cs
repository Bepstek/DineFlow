using dineflow.Controllers;
using System.Configuration;
using dineflow.Data;
using dineflow.Models;
using dineflow.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity
builder.Services.AddDefaultIdentity<IdentityUser>().AddDefaultTokenProviders()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IActivityLogger, LogsController>();
// Startup.cs or Program.cs


// Configure Cookie Authentication and Redirection After Login
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Identity/Account/Login"; // Redirect unauthenticated users to Login
//    options.AccessDeniedPath = "/Home"; // Redirect unauthorized users to Home

//    options.Events.OnRedirectToReturnUrl = context =>
//    {
//        if (!context.HttpContext.User.Identity.IsAuthenticated)
//        {
//            // If user is not logged in, send them to the login page
//            context.Response.Redirect(options.LoginPath);
//        }
//        else if (string.IsNullOrEmpty(context.Request.Query["returnUrl"]))
//        {
//            // If no return URL is provided, go to Dashboard
//            context.Response.Redirect("/Dashboard");
//        }
//        else
//        {
//            // Redirect to the original requested page
//            context.Response.Redirect(context.Request.Query["returnUrl"]);
//        }

//        return Task.CompletedTask;
//    };
//});


// Add controllers and views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ensure authentication middleware is enabled
app.UseAuthorization();

// Set up default route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "dashboard",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "dashboard",
    pattern: "{controller=Admin}/{action=Management}/{id?}");


// Map Razor Pages for Identity UI
app.MapRazorPages();

app.Run();
