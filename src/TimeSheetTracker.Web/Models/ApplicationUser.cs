using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TimeSheetTracker.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
