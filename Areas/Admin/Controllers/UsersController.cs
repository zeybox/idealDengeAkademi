using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Helpers;
using HizliOgren.Models;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;

    public UsersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, string? role, CancellationToken ct = default)
    {
        var query = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(u => u.Email.Contains(q) || u.FullName.Contains(q));
        if (!string.IsNullOrWhiteSpace(role) && int.TryParse(role, out var r)) query = query.Where(u => (int)u.Role == r);
        var list = await query.OrderByDescending(u => u.CreatedAt).ToListAsync(ct);
        var purchaseCounts = await _db.UserCourses.GroupBy(uc => uc.UserId).ToDictionaryAsync(g => g.Key, g => g.Count(), ct);
        ViewBag.PurchaseCounts = purchaseCounts;
        ViewBag.Q = q;
        ViewBag.Role = role;
        return View(list);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();
        ViewBag.Roles = new SelectList(new[] {
            new { Value = (int)UserRole.Uye, Text = "Üye" },
            new { Value = (int)UserRole.Egitmen, Text = "Eğitmen" },
            new { Value = (int)UserRole.Admin, Text = "Admin" }
        }, "Value", "Text", (int)user.Role);
        ViewBag.CityList = TurkishCities.GetCitySelectList(user.City);
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User model, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.Id, ct);
        if (user == null) return NotFound();

        user.FullName = model.FullName?.Trim() ?? "";
        user.Email = model.Email?.Trim() ?? "";
        user.City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim();
        user.Role = model.Role;
        user.Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim();
        user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(model.AvatarUrl) ? null : model.AvatarUrl.Trim();
        user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();

        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = "Üye bilgileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int id, UserRole role, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();
        user.Role = role;
        await _db.SaveChangesAsync(ct);
        TempData["Message"] = $"{user.FullName} rolü \"{role}\" olarak güncellendi.";
        return RedirectToAction(nameof(Index), new { q = Request.Query["q"], role = Request.Query["role"] });
    }
}
