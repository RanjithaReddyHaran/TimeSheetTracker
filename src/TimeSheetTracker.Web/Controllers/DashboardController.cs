using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeSheetTracker.Data;
using TimeSheetTracker.Models;

namespace TimeSheetTracker.Controllers;

[Authorize]
public class DashboardController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isReviewer = User.IsInRole("Admin") || User.IsInRole("Manager");
        var query = db.Timesheets.AsNoTracking().Include(x => x.Employee).Include(x => x.Entries);

        var sheets = isReviewer
            ? await query.OrderByDescending(x => x.WeekStartDate).Take(100).ToListAsync()
            : await query.Where(x => x.EmployeeId == userId).OrderByDescending(x => x.WeekStartDate).Take(30).ToListAsync();

        ViewBag.IsReviewer = isReviewer;
        ViewBag.PendingCount = isReviewer ? await db.Timesheets.CountAsync(x => x.Status == TimesheetStatus.Submitted) : 0;
        return View(sheets);
    }
}
