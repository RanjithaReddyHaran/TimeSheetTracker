using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeSheetTracker.Data;
using TimeSheetTracker.Models;
using TimeSheetTracker.Services;
using TimeSheetTracker.ViewModels;

namespace TimeSheetTracker.Controllers;

[Authorize]
public class TimesheetsController(
    ApplicationDbContext db,
    ITimesheetService timesheetService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(DateOnly? weekStart)
    {
        var monday = weekStart ?? GetMonday(DateOnly.FromDateTime(DateTime.Today));
        var sheet = await timesheetService.GetOrCreateDraftAsync(CurrentUserId, monday);
        return RedirectToAction(nameof(Details), new { id = sheet.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var sheet = await db.Timesheets.AsNoTracking()
            .Include(x => x.Entries).ThenInclude(x => x.Project)
            .Include(x => x.Employee)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (sheet is null) return NotFound();
        if (!CanView(sheet)) return Forbid();

        ViewBag.Projects = await db.ProjectAssignments.AsNoTracking()
            .Where(x => x.UserId == sheet.EmployeeId && x.Project.IsActive)
            .Select(x => x.Project).OrderBy(x => x.Name).ToListAsync();
        return View(sheet);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEntry(TimeEntryInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide valid time entry details.";
            return RedirectToAction(nameof(Details), new { id = model.TimesheetId });
        }

        try
        {
            await timesheetService.AddEntryAsync(CurrentUserId, model.TimesheetId, model.ProjectId,
                model.WorkDate, model.Hours, model.Description);
            TempData["Success"] = "Time entry added.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = model.TimesheetId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            await timesheetService.SubmitAsync(CurrentUserId, id);
            TempData["Success"] = "Timesheet submitted for approval.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewInputModel model, bool approve)
    {
        try
        {
            await timesheetService.ReviewAsync(CurrentUserId, model.TimesheetId, approve, model.Comment);
            TempData["Success"] = approve ? "Timesheet approved." : "Timesheet rejected.";
        }
        catch (Exception ex) when (ex is BusinessRuleException or KeyNotFoundException)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Index", "Dashboard");
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private bool CanView(Timesheet sheet) =>
        sheet.EmployeeId == CurrentUserId || User.IsInRole("Manager") || User.IsInRole("Admin");

    private static DateOnly GetMonday(DateOnly date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }
}
