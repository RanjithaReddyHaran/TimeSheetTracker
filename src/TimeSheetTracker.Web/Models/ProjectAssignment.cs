namespace TimeSheetTracker.Models;

public class ProjectAssignment
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime AssignedUtc { get; set; } = DateTime.UtcNow;
}
