using Crystal.Web.Data;
using Crystal.Web.Models;
using Crystal.Web.Services;
using Crystal.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Web.Controllers;

public class AccountController(ApplicationDbContext context, ISessionTokenService sessionTokenService) : Controller
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Tasks");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }

        var sessionTokens = await sessionTokenService.CreateSessionAsync(user);
        var principal = SessionTokenService.BuildPrincipal(user, sessionTokens.SessionId, sessionTokens.AccessToken);
        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = sessionTokens.RefreshTokenExpiresAt
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authenticationProperties);
        sessionTokenService.AppendRefreshTokenCookie(Response, sessionTokens.RefreshToken, sessionTokens.RefreshTokenExpiresAt);

        return RedirectToAction("Index", "Tasks");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await sessionTokenService.RevokeCurrentSessionAsync(User);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        sessionTokenService.DeleteRefreshTokenCookie(Response);
        return RedirectToAction(nameof(Login));
    }
}
