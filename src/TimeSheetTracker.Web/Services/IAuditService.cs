namespace TimeSheetTracker.Services;

public interface IAuditService
{
    Task WriteAsync(string action, string entityName, string entityId, string? details = null);
}
