using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Helpers;
using HizliOgren.Models;

namespace HizliOgren.Controllers;

/// <summary>Eğitmen ol başvuru formu — giriş yapmamış herkese ve öğrencilere açık; Admin/Eğitmen bu sayfaya gelirse yönlendirilir.</summary>
public class EgitmenBasvuruController : Controller
{
    private readonly ApplicationDbContext _db;

    public EgitmenBasvuruController(ApplicationDbContext db) => _db = db;

    private bool IsAdminOrEgitmen => User.IsInRole("Admin") || User.IsInRole("Egitmen");

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        if (IsAdminOrEgitmen)
            return RedirectToAction("Index", "Home");

        var model = new InstructorApplicationViewModel();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var id))
            {
                var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
                if (user != null)
                {
                    model.FullName = user.FullName ?? "";
                    model.Email = user.Email ?? "";
                    model.Phone = user.Phone;
                    model.City = user.City;
                }
            }
        }

        ViewBag.CityList = TurkishCities.GetCitySelectList(model.City);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(InstructorApplicationViewModel model, CancellationToken ct = default)
    {
        if (IsAdminOrEgitmen)
            return RedirectToAction("Index", "Home");

        ViewBag.CityList = TurkishCities.GetCitySelectList(model.City);

        var expectedCaptcha = HttpContext.Session.GetString("Captcha_Basvuru");
        HttpContext.Session.Remove("Captcha_Basvuru");
        var userCaptcha = (model.CaptchaCode ?? "").Trim();
        if (string.IsNullOrEmpty(expectedCaptcha) || !string.Equals(userCaptcha, expectedCaptcha, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("CaptchaCode", "Güvenlik kodu hatalı kontrol ediniz.");
        }

        if (!ModelState.IsValid)
            return View(model);

        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(uid) && int.TryParse(uid, out var id))
                userId = id;
        }

        var application = new InstructorApplication
        {
            FullName = (model.FullName ?? "").Trim(),
            Email = (model.Email ?? "").Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim(),
            Message = string.IsNullOrWhiteSpace(model.Message) ? null : model.Message.Trim(),
            UserId = userId,
            Status = InstructorApplicationStatus.Pending
        };
        _db.InstructorApplications.Add(application);
        await _db.SaveChangesAsync(ct);

        TempData["EgitmenBasvuruSuccess"] = true;
        return RedirectToAction(nameof(Index));
    }
}
