using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;
using HizliOgren.Services;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class InstructorApplicationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;

    public InstructorApplicationsController(ApplicationDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    public async Task<IActionResult> Index(string? status, CancellationToken ct = default)
    {
        IQueryable<InstructorApplication> query = _db.InstructorApplications
            .AsNoTracking()
            .Include(ia => ia.User);

        if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out var s))
            query = query.Where(ia => (int)ia.Status == s);

        var list = await query.OrderByDescending(ia => ia.CreatedAt).ToListAsync(ct);
        ViewBag.Status = status;
        return View(list);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var app = await _db.InstructorApplications
            .AsNoTracking()
            .Include(ia => ia.User)
            .FirstOrDefaultAsync(ia => ia.Id == id, ct);
        if (app == null) return NotFound();
        return View(app);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id, CancellationToken ct = default)
    {
        var app = await _db.InstructorApplications
            .Include(ia => ia.User)
            .FirstOrDefaultAsync(ia => ia.Id == id, ct);
        if (app == null) return NotFound();
        if (app.Status != InstructorApplicationStatus.Pending)
        {
            TempData["Message"] = "Bu başvuru zaten işlenmiş.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var password = Random.Shared.Next(10000, 100000).ToString();
        var email = app.Email.Trim().ToLowerInvariant();

        if (app.UserId.HasValue)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == app.UserId.Value, ct);
            if (user != null)
            {
                user.Role = UserRole.Egitmen;
                user.PasswordHash = _authService.HashPassword(password);
                user.PlainPassword = password;
                user.FullName = app.FullName?.Trim() ?? user.FullName;
                user.Phone = string.IsNullOrWhiteSpace(app.Phone) ? user.Phone : app.Phone.Trim();
                user.City = string.IsNullOrWhiteSpace(app.City) ? user.City : app.City.Trim();
                app.Status = InstructorApplicationStatus.Approved;
                await _db.SaveChangesAsync(ct);
                TempData["Message"] = $"Başvuru kabul edildi. Üye #{user.Id} ({user.FullName}) eğitmen yapıldı. Yeni giriş şifresi: {password}";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email, ct);
        if (existingUser != null)
        {
            existingUser.Role = UserRole.Egitmen;
            existingUser.PasswordHash = _authService.HashPassword(password);
            existingUser.PlainPassword = password;
            existingUser.FullName = string.IsNullOrWhiteSpace(app.FullName) ? existingUser.FullName : app.FullName.Trim();
            existingUser.Phone = string.IsNullOrWhiteSpace(app.Phone) ? existingUser.Phone : app.Phone.Trim();
            existingUser.City = string.IsNullOrWhiteSpace(app.City) ? existingUser.City : app.City.Trim();
            app.UserId = existingUser.Id;
            app.Status = InstructorApplicationStatus.Approved;
            await _db.SaveChangesAsync(ct);
            TempData["Message"] = $"Başvuru kabul edildi. Mevcut üye #{existingUser.Id} eğitmen yapıldı. Giriş şifresi: {password}";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var newUser = new User
        {
            Email = email,
            FullName = (app.FullName ?? "").Trim(),
            Phone = string.IsNullOrWhiteSpace(app.Phone) ? null : app.Phone.Trim(),
            City = string.IsNullOrWhiteSpace(app.City) ? null : app.City.Trim(),
            PasswordHash = _authService.HashPassword(password),
            PlainPassword = password,
            Role = UserRole.Egitmen,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync(ct);
        app.UserId = newUser.Id;
        app.Status = InstructorApplicationStatus.Approved;
        await _db.SaveChangesAsync(ct);
        TempData["Message"] = $"Başvuru kabul edildi. Yeni eğitmen üye oluşturuldu (#{newUser.Id}). Giriş şifresi: {password}";
        return RedirectToAction(nameof(Detail), new { id });
    }
}
