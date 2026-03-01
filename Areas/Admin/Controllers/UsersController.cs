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

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Roles = new SelectList(new[] {
            new { Value = (int)UserRole.Uye, Text = "Üye" },
            new { Value = (int)UserRole.Egitmen, Text = "Eğitmen" },
            new { Value = (int)UserRole.Admin, Text = "Admin" }
        }, "Value", "Text", (int)UserRole.Uye);
        ViewBag.CityList = TurkishCities.GetCitySelectList(null);
        return View(new AdminUserCreateModel { Role = UserRole.Uye });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateModel model, CancellationToken ct = default)
    {
        var email = (model.Email ?? "").Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email, ct))
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new SelectList(new[] {
                new { Value = (int)UserRole.Uye, Text = "Üye" },
                new { Value = (int)UserRole.Egitmen, Text = "Eğitmen" },
                new { Value = (int)UserRole.Admin, Text = "Admin" }
            }, "Value", "Text", (int)model.Role);
            ViewBag.CityList = TurkishCities.GetCitySelectList(model.City);
            return View(model);
        }

        var user = new User
        {
            Email = email,
            FullName = (model.FullName ?? "").Trim(),
            PasswordHash = _authService.HashPassword((model.Password ?? "").Trim()),
            PlainPassword = (model.Password ?? "").Trim(),
            Role = model.Role,
            City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim(),
            Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim(),
            Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim(),
            AvatarUrl = string.IsNullOrWhiteSpace(model.AvatarUrl) ? null : model.AvatarUrl.Trim(),
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        TempData["Message"] = $"{user.FullName} kullanıcı olarak eklendi.";
        return RedirectToAction(nameof(Index));
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();

        if (await _db.Courses.AnyAsync(c => c.InstructorId == id, ct))
        {
            TempData["Message"] = "Bu kullanıcı eğitmen olarak atanmış eğitimlere sahip. Önce eğitimlerin eğitmenini değiştirin.";
            return RedirectToAction(nameof(Index));
        }

        var orders = await _db.Orders.Where(o => o.UserId == id).ToListAsync(ct);
        foreach (var order in orders)
        {
            var items = await _db.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync(ct);
            _db.OrderItems.RemoveRange(items);
        }
        _db.Orders.RemoveRange(orders);
        _db.UserCourses.RemoveRange(await _db.UserCourses.Where(uc => uc.UserId == id).ToListAsync(ct));
        var apps = await _db.InstructorApplications.Where(ia => ia.UserId == id).ToListAsync(ct);
        foreach (var app in apps) app.UserId = null;
        var posts = await _db.BlogPosts.Where(b => b.AuthorId == id).ToListAsync(ct);
        foreach (var post in posts) post.AuthorId = null;
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
        TempData["Message"] = $"{user.FullName} kullanıcısı silindi.";
        return RedirectToAction(nameof(Index));
    }
}
