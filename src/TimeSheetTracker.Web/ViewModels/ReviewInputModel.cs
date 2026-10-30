using System.ComponentModel.DataAnnotations;

namespace TimeSheetTracker.ViewModels;

public class ReviewInputModel
{
    public int TimesheetId { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}
