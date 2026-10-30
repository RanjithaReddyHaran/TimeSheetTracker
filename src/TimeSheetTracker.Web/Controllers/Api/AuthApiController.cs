using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeSheetTracker.Models;

namespace TimeSheetTracker.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController(SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
    {
        if (User.Identity?.IsAuthenticated != true) return Unauthorized();
        return Ok(new { User.Identity.Name, Roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive) return Unauthorized(new { message = "Invalid sign-in attempt." });
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded) return Unauthorized(new { message = result.IsLockedOut ? "Account temporarily locked." : "Invalid sign-in attempt." });
        return Ok(new { user.Email, user.FullName });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }
}

public record LoginRequest(string Email, string Password, bool RememberMe = false);

