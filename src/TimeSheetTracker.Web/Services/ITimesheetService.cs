using TimeSheetTracker.Models;

namespace TimeSheetTracker.Services;

public interface ITimesheetService
{
    Task<Timesheet> GetOrCreateDraftAsync(string employeeId, DateOnly weekStart);
    Task AddEntryAsync(string employeeId, int timesheetId, int projectId, DateOnly workDate, decimal hours, string description);
    Task SubmitAsync(string employeeId, int timesheetId);
    Task ReviewAsync(string reviewerId, int timesheetId, bool approve, string? comment);
}
