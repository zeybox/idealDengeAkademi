using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;

namespace HizliOgren.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _db;

    public CoursesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? cat, string? q, CancellationToken ct = default)
    {
        var query = _db.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.CurriculumItems)
            .Where(c => c.IsPublished)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(cat))
            query = query.Where(c => c.Category.Slug == cat);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Title.Contains(q) || (c.Description != null && c.Description.Contains(q)));

        var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.SortOrder).AsNoTracking().ToListAsync(ct);
        ViewBag.CurrentCat = cat;
        ViewBag.Q = q;
        return View(list);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var course = await _db.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.CurriculumItems.OrderBy(x => x.SortOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsPublished, ct);
        if (course == null) return NotFound();

        var userId = User.Identity?.IsAuthenticated == true ? int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value) : 0;
        var hasAccess = userId > 0 && await _db.UserCourses.AnyAsync(uc => uc.UserId == userId && uc.CourseId == id, ct);
        ViewBag.HasAccess = hasAccess;
        return View(course);
    }

    [Authorize]
    public async Task<IActionResult> Watch(int id, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var hasAccess = await _db.UserCourses.AnyAsync(uc => uc.UserId == userId && uc.CourseId == id, ct);
        if (!hasAccess) return Forbid();

        var course = await _db.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.CurriculumItems.OrderBy(x => x.SortOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course == null) return NotFound();
        return View(course);
    }
}
