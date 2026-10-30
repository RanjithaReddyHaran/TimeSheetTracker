using System.Security.Claims;
using TimeSheetTracker.Data;
using TimeSheetTracker.Models;

namespace TimeSheetTracker.Services;

public class AuditService(ApplicationDbContext db, IHttpContextAccessor accessor) : IAuditService
{
    public async Task WriteAsync(string action, string entityName, string entityId, string? details = null)
    {
        var context = accessor.HttpContext;
        db.AuditLogs.Add(new AuditLog
        {
            ActorUserId = context?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            CorrelationId = context?.TraceIdentifier
        });
        await db.SaveChangesAsync();
    }
}
