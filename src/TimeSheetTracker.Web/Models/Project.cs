using System.ComponentModel.DataAnnotations;

namespace TimeSheetTracker.Models;

public class Project
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string ProjectCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
}
