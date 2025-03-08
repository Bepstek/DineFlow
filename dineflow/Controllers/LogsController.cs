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
using dineflow.Services;
namespace dineflow.Controllers
{
    public class LogsController : Controller, IActivityLogger
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public void ActivityLog(string action, string userId, string detail)
        {
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = action,
                Details = detail,
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);
            _context.SaveChanges();
        }
    }
}
