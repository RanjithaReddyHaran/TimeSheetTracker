namespace TimeSheetTracker.Models;

public enum TimesheetStatus { Draft = 0, Submitted = 1, Approved = 2, Rejected = 3 }

public class Timesheet
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public ApplicationUser Employee { get; set; } = null!;
    public DateOnly WeekStartDate { get; set; }
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedUtc { get; set; }
    public DateTime? ReviewedUtc { get; set; }
    public string? ReviewedById { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }
    public string? ReviewComment { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<TimeEntry> Entries { get; set; } = new List<TimeEntry>();
}
