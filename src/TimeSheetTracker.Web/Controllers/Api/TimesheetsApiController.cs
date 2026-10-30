using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeSheetTracker.Data;
using TimeSheetTracker.Models;
using TimeSheetTracker.Services;

namespace TimeSheetTracker.Controllers.Api;

[ApiController]
[Route("api/timesheets")]
[Authorize]
public class TimesheetsApiController(ApplicationDbContext db, ITimesheetService service) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var query = db.Timesheets.AsNoTracking()
            .Include(x => x.Employee).Include(x => x.Entries);
        var sheets = User.IsInRole("Admin") || User.IsInRole("Manager")
            ? await query.OrderByDescending(x => x.WeekStartDate).Take(100).ToListAsync(ct)
            : await query.Where(x => x.EmployeeId == UserId).OrderByDescending(x => x.WeekStartDate).Take(30).ToListAsync(ct);

        return Ok(sheets.Select(x => new
        {
            x.Id, x.WeekStartDate, Status = x.Status.ToString(),
            EmployeeName = x.Employee.FullName,
            TotalHours = x.Entries.Sum(e => e.Hours),
            EntryCount = x.Entries.Count
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var sheet = await db.Timesheets.AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.Entries).ThenInclude(x => x.Project)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (sheet is null) return NotFound();
        if (sheet.EmployeeId != UserId && !User.IsInRole("Manager") && !User.IsInRole("Admin")) return Forbid();

        return Ok(new
        {
            sheet.Id, sheet.WeekStartDate, Status = sheet.Status.ToString(),
            EmployeeName = sheet.Employee.FullName, sheet.ReviewComment,
            TotalHours = sheet.Entries.Sum(e => e.Hours),
            Entries = sheet.Entries.OrderBy(e => e.WorkDate).Select(e => new
            {
                e.Id, e.WorkDate, e.Hours, e.Description,
                ProjectId = e.ProjectId, ProjectName = e.Project.Name, ProjectCode = e.Project.ProjectCode
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimesheetRequest request)
    {
        var sheet = await service.GetOrCreateDraftAsync(UserId, request.WeekStartDate);
        return CreatedAtAction(nameof(Get), new { id = sheet.Id }, new { sheet.Id });
    }

    [HttpPost("{id:int}/entries")]
    public async Task<IActionResult> AddEntry(int id, [FromBody] AddEntryRequest request)
    {
        await service.AddEntryAsync(UserId, id, request.ProjectId, request.WorkDate,
            request.Hours, request.Description);
        return NoContent();
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        await service.SubmitAsync(UserId, id);
        return NoContent();
    }

    [HttpPost("{id:int}/review")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewRequest request)
    {
        await service.ReviewAsync(UserId, id, request.Approve, request.Comment);
        return NoContent();
    }

    [HttpGet("projects")]
    public async Task<IActionResult> Projects(CancellationToken ct)
    {
        var projects = await db.ProjectAssignments.AsNoTracking()
            .Where(x => x.UserId == UserId && x.Project.IsActive)
            .Select(x => new { x.Project.Id, x.Project.ProjectCode, x.Project.Name })
            .OrderBy(x => x.Name).ToListAsync(ct);
        return Ok(projects);
    }
}

public record CreateTimesheetRequest(DateOnly WeekStartDate);
public record AddEntryRequest(int ProjectId, DateOnly WorkDate, decimal Hours, string Description);
public record ReviewRequest(bool Approve, string? Comment);

