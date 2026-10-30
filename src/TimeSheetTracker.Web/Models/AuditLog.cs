namespace TimeSheetTracker.Models;

public class AuditLog
{
    public long Id { get; set; }
    public string? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}
