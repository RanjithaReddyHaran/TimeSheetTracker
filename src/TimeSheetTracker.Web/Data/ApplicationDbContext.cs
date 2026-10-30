using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeSheetTracker.Models;

namespace TimeSheetTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Project>().HasIndex(x => x.ProjectCode).IsUnique();
        builder.Entity<Project>().Property(x => x.ProjectCode).HasMaxLength(30);
        builder.Entity<Project>().Property(x => x.Name).HasMaxLength(150);

        builder.Entity<ProjectAssignment>()
            .HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
        builder.Entity<ProjectAssignment>()
            .HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Timesheet>()
            .HasIndex(x => new { x.EmployeeId, x.WeekStartDate }).IsUnique();
        builder.Entity<Timesheet>().Property(x => x.RowVersion).IsRowVersion();
        builder.Entity<Timesheet>()
            .HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Timesheet>()
            .HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TimeEntry>().Property(x => x.Hours).HasPrecision(5, 2);
        builder.Entity<TimeEntry>()
            .HasIndex(x => new { x.TimesheetId, x.WorkDate, x.ProjectId });
        builder.Entity<TimeEntry>()
            .HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AuditLog>().HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedUtc });
        builder.Entity<AuditLog>().Property(x => x.Action).HasMaxLength(100);
        builder.Entity<AuditLog>().Property(x => x.EntityName).HasMaxLength(100);
    }
}
