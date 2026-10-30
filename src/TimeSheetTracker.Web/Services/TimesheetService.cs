using Microsoft.EntityFrameworkCore;
using TimeSheetTracker.Data;
using TimeSheetTracker.Models;

namespace TimeSheetTracker.Services;

public class TimesheetService(ApplicationDbContext db, IAuditService audit) : ITimesheetService
{
    public async Task<Timesheet> GetOrCreateDraftAsync(string employeeId, DateOnly weekStart)
    {
        EnsureMonday(weekStart);
        var sheet = await db.Timesheets.Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.WeekStartDate == weekStart);

        if (sheet is not null) return sheet;

        sheet = new Timesheet { EmployeeId = employeeId, WeekStartDate = weekStart };
        db.Timesheets.Add(sheet);
        await db.SaveChangesAsync();
        await audit.WriteAsync("Created", nameof(Timesheet), sheet.Id.ToString());
        return sheet;
    }

    public async Task AddEntryAsync(string employeeId, int timesheetId, int projectId,
        DateOnly workDate, decimal hours, string description)
    {
        if (hours < 0.25m || hours > 24m || decimal.Round(hours, 2) != hours)
            throw new BusinessRuleException("Hours must be between 0.25 and 24.00, in increments of 0.01.");
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 1000)
            throw new BusinessRuleException("A task description between 1 and 1000 characters is required.");

        var sheet = await db.Timesheets.Include(x => x.Entries)
            .SingleOrDefaultAsync(x => x.Id == timesheetId && x.EmployeeId == employeeId)
            ?? throw new KeyNotFoundException("Timesheet was not found.");

        EnsureEditable(sheet);
        if (workDate < sheet.WeekStartDate || workDate > sheet.WeekStartDate.AddDays(6))
            throw new BusinessRuleException("The work date must fall within the selected timesheet week.");

        var assigned = await db.ProjectAssignments.AnyAsync(x => x.ProjectId == projectId &&
            x.UserId == employeeId && x.Project.IsActive);
        if (!assigned) throw new BusinessRuleException("You are not assigned to this active project.");

        var dailyTotal = sheet.Entries.Where(x => x.WorkDate == workDate).Sum(x => x.Hours);
        if (dailyTotal + hours > 24m)
            throw new BusinessRuleException("Total hours for a work date cannot exceed 24.");

        db.TimeEntries.Add(new TimeEntry
        {
            TimesheetId = sheet.Id, ProjectId = projectId, WorkDate = workDate,
            Hours = hours, Description = description.Trim()
        });
        await db.SaveChangesAsync();
        await audit.WriteAsync("EntryAdded", nameof(TimeEntry), timesheetId.ToString(),
            $"ProjectId={projectId}; WorkDate={workDate}; Hours={hours}");
    }

    public async Task SubmitAsync(string employeeId, int timesheetId)
    {
        var sheet = await db.Timesheets.Include(x => x.Entries)
            .SingleOrDefaultAsync(x => x.Id == timesheetId && x.EmployeeId == employeeId)
            ?? throw new KeyNotFoundException("Timesheet was not found.");

        EnsureEditable(sheet);
        if (sheet.Entries.Count == 0) throw new BusinessRuleException("Add at least one time entry before submitting.");
        if (sheet.Entries.Any(x => x.Hours <= 0))
            throw new BusinessRuleException("Timesheet contains an invalid time entry.");

        sheet.Status = TimesheetStatus.Submitted;
        sheet.SubmittedUtc = DateTime.UtcNow;
        sheet.ReviewComment = null;
        await db.SaveChangesAsync();
        await audit.WriteAsync("Submitted", nameof(Timesheet), sheet.Id.ToString());
    }

    public async Task ReviewAsync(string reviewerId, int timesheetId, bool approve, string? comment)
    {
        var sheet = await db.Timesheets.SingleOrDefaultAsync(x => x.Id == timesheetId)
            ?? throw new KeyNotFoundException("Timesheet was not found.");

        if (sheet.Status != TimesheetStatus.Submitted)
            throw new BusinessRuleException("Only submitted timesheets can be reviewed.");
        if (!approve && string.IsNullOrWhiteSpace(comment))
            throw new BusinessRuleException("A comment is required when rejecting a timesheet.");

        sheet.Status = approve ? TimesheetStatus.Approved : TimesheetStatus.Rejected;
        sheet.ReviewedById = reviewerId;
        sheet.ReviewedUtc = DateTime.UtcNow;
        sheet.ReviewComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        await db.SaveChangesAsync();
        await audit.WriteAsync(approve ? "Approved" : "Rejected", nameof(Timesheet), sheet.Id.ToString(), sheet.ReviewComment);
    }

    private static void EnsureEditable(Timesheet sheet)
    {
        if (sheet.Status is not (TimesheetStatus.Draft or TimesheetStatus.Rejected))
            throw new BusinessRuleException("Only draft or rejected timesheets can be modified.");
    }

    private static void EnsureMonday(DateOnly date)
    {
        if (date.DayOfWeek != DayOfWeek.Monday)
            throw new BusinessRuleException("Timesheet week must start on a Monday.");
    }
}

public sealed class BusinessRuleException(string message) : Exception(message);
