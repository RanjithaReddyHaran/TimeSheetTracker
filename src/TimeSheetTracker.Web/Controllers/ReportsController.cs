using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeSheetTracker.Data;
using TimeSheetTracker.Models;

namespace TimeSheetTracker.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class ReportsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to)
    {
        var start = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var end = to ?? DateOnly.FromDateTime(DateTime.Today);
        if (end < start || end.DayNumber - start.DayNumber > 366)
        {
            ModelState.AddModelError(string.Empty, "Select a valid date range of no more than 366 days.");
            end = start.AddDays(30);
        }

        var rows = await db.TimeEntries.AsNoTracking()
            .Where(x => x.WorkDate >= start && x.WorkDate <= end && x.Timesheet.Status == TimesheetStatus.Approved)
            .GroupBy(x => new { x.Timesheet.Employee.FullName, x.Project.Name })
            .Select(g => new ReportRow(g.Key.FullName, g.Key.Name, g.Sum(x => x.Hours)))
            .OrderBy(x => x.EmployeeName).ThenBy(x => x.ProjectName).ToListAsync();

        ViewBag.From = start;
        ViewBag.To = end;
        return View(rows);
    }
}

public record ReportRow(string EmployeeName, string ProjectName, decimal TotalHours);
