using System.ComponentModel.DataAnnotations;

namespace TimeSheetTracker.ViewModels;

public class TimeEntryInputModel
{
    [Required]
    public int TimesheetId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public DateOnly WorkDate { get; set; }

    [Range(typeof(decimal), "0.25", "24.00")]
    public decimal Hours { get; set; }

    [Required, StringLength(1000, MinimumLength = 3)]
    public string Description { get; set; } = string.Empty;
}
