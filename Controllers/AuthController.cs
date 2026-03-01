using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HizliOgren.Data;
using HizliOgren.Helpers;
using HizliOgren.Models;
using HizliOgren.Services;
using Microsoft.EntityFrameworkCore;

namespace HizliOgren.Controllers;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(ApplicationDbContext db, AuthService authService, IConfiguration config)
    {
        _db = db;
        _authService = authService;
        _config = config;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl ?? "/";
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Lütfen e-posta ve şifre alanlarını doldurun.");
            return View(model);
        }

        var email = (model.Email ?? "").Trim().ToLowerInvariant();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email.ToLower() == email, ct);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }
        if (!_authService.VerifyPassword((model.Password ?? "").Trim(), user.PasswordHash))
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        await SignInAsync(user);
        var redirect = string.IsNullOrWhiteSpace(returnUrl) || returnUrl.Contains("/Auth/Login", StringComparison.OrdinalIgnoreCase) || returnUrl.Contains("/Auth/Register", StringComparison.OrdinalIgnoreCase)
            ? "/"
            : returnUrl;
        return LocalRedirect(redirect);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewBag.CityList = TurkishCities.GetCitySelectList(null);
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewBag.CityList = TurkishCities.GetCitySelectList(model.City);

        var expectedCaptcha = HttpContext.Session.GetString("Captcha_Register");
        HttpContext.Session.Remove("Captcha_Register");
        var userCaptcha = (model.CaptchaCode ?? "").Trim();
        if (string.IsNullOrEmpty(expectedCaptcha) || !string.Equals(userCaptcha, expectedCaptcha, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("CaptchaCode", "Güvenlik kodu hatalı kontrol ediniz.");
        }

        if (!ModelState.IsValid) return View(model);

        var email = (model.Email ?? "").Trim().ToLowerInvariant();
        var password = (model.Password ?? "").Trim();
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email, ct))
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
            return View(model);
        }

        var user = new User
        {
            Email = email,
            FullName = (model.FullName ?? "").Trim(),
            City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim(),
            PasswordHash = _authService.HashPassword(password),
            PlainPassword = password,
            Role = UserRole.Uye,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        await SignInAsync(user);
        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Google(string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(_config["Google:ClientId"]))
        {
            TempData["Message"] = "Google ile giriş henüz yapılandırılmamış. Admin panelinden Ayarlar → Google Giriş ile ekleyebilirsiniz.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }
        var redirectUri = Url.Action(nameof(ExternalLoginCallback), null, new { returnUrl = returnUrl ?? "/" }, Request.Scheme);
        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        return Challenge(props, "Google");
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, CancellationToken ct = default)
    {
        var result = await HttpContext.AuthenticateAsync("Google");
        if (!result.Succeeded) return RedirectToAction(nameof(Login));

        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var name = result.Principal?.FindFirstValue(ClaimTypes.Name);
        var googleId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(email)) return RedirectToAction(nameof(Login));

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email || u.GoogleId == googleId, ct);
        if (user == null)
        {
            user = new User { Email = email, FullName = name ?? email, GoogleId = googleId, Role = UserRole.Uye };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }
        else if (string.IsNullOrEmpty(user.GoogleId))
        {
            user.GoogleId = googleId;
            await _db.SaveChangesAsync(ct);
        }

        await HttpContext.SignOutAsync("Google");
        await SignInAsync(user);
        return LocalRedirect(returnUrl ?? "/");
    }

    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
