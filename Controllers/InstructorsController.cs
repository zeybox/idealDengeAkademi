using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;

namespace HizliOgren.Controllers;

public class InstructorsController : Controller
{
    private readonly ApplicationDbContext _db;

    public InstructorsController(ApplicationDbContext db) => _db = db;

    /// <summary>Herkese açık eğitmen profil sayfası.</summary>
    public async Task<IActionResult> Profile(int id, CancellationToken ct = default)
    {
        var instructor = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Egitmen, ct);
        if (instructor == null) return NotFound();

        var courses = await _db.Courses
            .Where(c => c.InstructorId == id && c.IsPublished)
            .Include(c => c.Category)
            .Include(c => c.CurriculumItems)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

        ViewBag.Courses = courses;
        return View(instructor);
    }
}
