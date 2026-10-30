using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeSheetTracker.Models;

namespace TimeSheetTracker.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger, bool isDevelopment)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(scope.ServiceProvider, db, logger);
    }

    public static async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await SeedAsync(scope.ServiceProvider, db, scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer"));
    }

    private static async Task SeedAsync(IServiceProvider services, ApplicationDbContext db, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Admin", "Manager", "Employee" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Demo accounts are created only in Development. Change passwords immediately if reused.
        if (isDevelopment)
        {
            await CreateDemoUser(userManager, "admin@timesheet.local", "System Admin", "Admin");
            await CreateDemoUser(userManager, "manager@timesheet.local", "Morgan Manager", "Manager");
            await CreateDemoUser(userManager, "employee@timesheet.local", "Evan Employee", "Employee");
        }

        if (!await db.Projects.AnyAsync())
        {
            db.Projects.AddRange(
                new Project { ProjectCode = "PRJ-100", Name = "Customer Portal", Description = "Customer self-service portal" },
                new Project { ProjectCode = "PRJ-200", Name = "Internal Platform", Description = "Internal engineering platform" });
            await db.SaveChangesAsync();
        }

        var employee = await userManager.FindByEmailAsync("employee@timesheet.local");
        if (employee is not null)
        {
            foreach (var project in await db.Projects.ToListAsync())
            {
                if (!await db.ProjectAssignments.AnyAsync(x => x.UserId == employee.Id && x.ProjectId == project.Id))
                    db.ProjectAssignments.Add(new ProjectAssignment { UserId = employee.Id, ProjectId = project.Id });
            }
            await db.SaveChangesAsync();
        }

        logger.LogInformation("Database initialization completed");
    }

    private static async Task CreateDemoUser(UserManager<ApplicationUser> manager, string email, string name, string role)
    {
        var user = await manager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = name };
            var result = await manager.CreateAsync(user, "Demo!Pass12345");
            if (!result.Succeeded)
                throw new InvalidOperationException($"Unable to create demo user {email}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
        }

        if (!await manager.IsInRoleAsync(user, role))
            await manager.AddToRoleAsync(user, role);
    }
}
