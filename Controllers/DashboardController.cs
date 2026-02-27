using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;

namespace HizliOgren.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var courses = await _db.UserCourses
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Course)
                .ThenInclude(c => c.Category)
            .Include(uc => uc.Course)
                .ThenInclude(c => c.Instructor)
            .AsNoTracking()
            .Select(uc => uc.Course)
            .ToListAsync(ct);
        return View(courses);
    }
}
