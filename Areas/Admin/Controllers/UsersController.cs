using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Helpers;
using HizliOgren.Models;
using HizliOgren.Services;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _authService;

    public UsersController(ApplicationDbContext db, AuthService authService)
    {
        _db = db;
        _authService = authService;
    }

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
        var model = new AdminUserEditModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            City = user.City,
            Title = user.Title,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            Password = user.PlainPassword ?? ""
        };
        ViewBag.Roles = new SelectList(new[] {
            new { Value = (int)UserRole.Uye, Text = "Üye" },
            new { Value = (int)UserRole.Egitmen, Text = "Eğitmen" },
            new { Value = (int)UserRole.Admin, Text = "Admin" }
        }, "Value", "Text", (int)user.Role);
        ViewBag.CityList = TurkishCities.GetCitySelectList(user.City);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminUserEditModel model, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.Id, ct);
        if (user == null) return NotFound();

        void SetEditViewBag()
        {
            ViewBag.Roles = new SelectList(new[] {
                new { Value = (int)UserRole.Uye, Text = "Üye" },
                new { Value = (int)UserRole.Egitmen, Text = "Eğitmen" },
                new { Value = (int)UserRole.Admin, Text = "Admin" }
            }, "Value", "Text", (int)model.Role);
            ViewBag.CityList = TurkishCities.GetCitySelectList(model.City);
        }

        if (!ModelState.IsValid)
        {
            SetEditViewBag();
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = _authService.HashPassword(model.Password.Trim());
            user.PlainPassword = model.Password.Trim();
        }

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

    /// <summary>Üyenin satın aldığı eğitimleri yönet: ekle veya kaldır.</summary>
    public async Task<IActionResult> EditCourses(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();

        var purchased = await _db.UserCourses
            .AsNoTracking()
            .Where(uc => uc.UserId == id)
            .Include(uc => uc.Course)
            .OrderBy(uc => uc.Course.Title)
            .Select(uc => new UserCourseItem
            {
                UserCourseId = uc.Id,
                CourseId = uc.CourseId,
                Title = uc.Course.Title
            })
            .ToListAsync(ct);

        var purchasedIds = purchased.Select(p => p.CourseId).ToHashSet();
        var availableToAdd = await _db.Courses
            .AsNoTracking()
            .Where(c => !purchasedIds.Contains(c.Id))
            .OrderBy(c => c.Title)
            .Select(c => new CourseItem { Id = c.Id, Title = c.Title })
            .ToListAsync(ct);

        var model = new AdminUserCoursesModel
        {
            UserId = user.Id,
            UserFullName = user.FullName,
            Purchased = purchased,
            AvailableToAdd = availableToAdd
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCourse(int id, int courseId, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();
        var exists = await _db.UserCourses.AnyAsync(uc => uc.UserId == id && uc.CourseId == courseId, ct);
        if (!exists)
        {
            _db.UserCourses.Add(new UserCourse { UserId = id, CourseId = courseId });
            await _db.SaveChangesAsync(ct);
        }
        TempData["Toast"] = "Eğitim eklendi.";
        return RedirectToAction(nameof(EditCourses), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCourse(int id, int userCourseId, CancellationToken ct = default)
    {
        var uc = await _db.UserCourses.FirstOrDefaultAsync(x => x.Id == userCourseId && x.UserId == id, ct);
        if (uc != null)
        {
            _db.UserCourses.Remove(uc);
            await _db.SaveChangesAsync(ct);
            TempData["Toast"] = "Eğitim kaldırıldı.";
        }
        return RedirectToAction(nameof(EditCourses), new { id });
    }
}
