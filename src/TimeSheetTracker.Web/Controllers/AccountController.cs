using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeSheetTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace TimeSheetTracker.Controllers;

public class AccountController(SignInManager<ApplicationUser> signInManager) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid sign-in attempt.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Dashboard");

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Your account is temporarily locked. Try again later."
            : "Invalid sign-in attempt.");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}

public class LoginModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
