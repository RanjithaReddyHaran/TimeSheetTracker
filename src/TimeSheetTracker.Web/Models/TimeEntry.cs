using System.ComponentModel.DataAnnotations;

namespace TimeSheetTracker.Models;

public class TimeEntry
{
    public int Id { get; set; }
    public int TimesheetId { get; set; }
    public Timesheet Timesheet { get; set; } = null!;
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public DateOnly WorkDate { get; set; }

    [Range(typeof(decimal), "0.25", "24.00")]
    public decimal Hours { get; set; }

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedUtc { get; set; }
}
