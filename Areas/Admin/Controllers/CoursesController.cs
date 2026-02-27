using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _db;

    public CoursesController(ApplicationDbContext db) => _db = db;

    private static string EnsureLinksBlank(string? html)
    {
        if (string.IsNullOrEmpty(html)) return html ?? "";
        html = Regex.Replace(html, @"\btarget\s*=\s*[""'][^""']*[""']", "target=\"_blank\"", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<a\s+([^>]*?)>", m =>
        {
            var attrs = m.Groups[1].Value;
            if (attrs.Contains("target=", StringComparison.OrdinalIgnoreCase)) return m.Value;
            return "<a target=\"_blank\" " + attrs + ">";
        }, RegexOptions.IgnoreCase);
        return html;
    }

    public async Task<IActionResult> Index(string? q, int? catId, bool? published, CancellationToken ct = default)
    {
        var query = _db.Courses.Include(c => c.Category).Include(c => c.Instructor).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Title.Contains(q));
        if (catId.HasValue) query = query.Where(c => c.CategoryId == catId.Value);
        if (published.HasValue) query = query.Where(c => c.IsPublished == published.Value);
        var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync(ct), "Id", "Name", catId);
        ViewBag.Q = q;
        ViewBag.Published = published;
        return View(list);
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken ct = default)
    {
        ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync(ct), "Id", "Name");
        ViewBag.Instructors = new SelectList(await _db.Users.Where(u => u.Role == UserRole.Egitmen || u.Role == UserRole.Admin).ToListAsync(ct), "Id", "FullName");
        if (!id.HasValue) return View(new Course { IsPublished = false });
        var course = await _db.Courses.Include(c => c.CurriculumItems.OrderBy(x => x.SortOrder)).FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course == null) return NotFound();
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Course model, CancellationToken ct = default)
    {
        ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync(ct), "Id", "Name");
        ViewBag.Instructors = new SelectList(await _db.Users.Where(u => u.Role == UserRole.Egitmen || u.Role == UserRole.Admin).ToListAsync(ct), "Id", "FullName");
        model.Description = EnsureLinksBlank(model.Description);
        model.PaidContent = EnsureLinksBlank(model.PaidContent);
        if (model.Id == 0)
        {
            model.CreatedAt = DateTime.UtcNow;
            if (model.InstructorId == 0)
                model.InstructorId = await _db.Users.Where(u => u.Role == UserRole.Admin).Select(u => u.Id).FirstAsync(ct);
            _db.Courses.Add(model);
            await _db.SaveChangesAsync(ct);
            TempData["Toast"] = "Eğitim eklendi.";
            return RedirectToAction(nameof(Index));
        }
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == model.Id, ct);
        if (course == null) return NotFound();
        course.Title = model.Title;
        course.Description = model.Description;
        course.Highlights = model.Highlights;
        course.ContentSummary = model.ContentSummary;
        course.PaidContent = model.PaidContent;
        course.CategoryId = model.CategoryId;
        course.InstructorId = model.InstructorId > 0 ? model.InstructorId : course.InstructorId;
        course.Price = model.Price;
        course.ImageUrl = model.ImageUrl;
        course.IsPublished = model.IsPublished;
        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = "Eğitim güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        _db.CurriculumItems.RemoveRange(await _db.CurriculumItems.Where(ci => ci.CourseId == id).ToListAsync(ct));
        _db.OrderItems.RemoveRange(await _db.OrderItems.Where(oi => oi.CourseId == id).ToListAsync(ct));
        _db.UserCourses.RemoveRange(await _db.UserCourses.Where(uc => uc.CourseId == id).ToListAsync(ct));
        _db.Courses.Remove(course);
        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = "Eğitim silindi.";
        return RedirectToAction(nameof(Index));
    }
}
