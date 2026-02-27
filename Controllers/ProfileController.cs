using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Helpers;
using HizliOgren.Models;

namespace HizliOgren.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProfileController(ApplicationDbContext db) => _db = db;

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return NotFound();
        ViewBag.CityList = TurkishCities.GetCitySelectList(user.City);
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(User model, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return NotFound();

        user.FullName = model.FullName?.Trim() ?? "";
        user.City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim();
        user.Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim();
        user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(model.AvatarUrl) ? null : model.AvatarUrl.Trim();
        user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();

        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = "Profil güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
