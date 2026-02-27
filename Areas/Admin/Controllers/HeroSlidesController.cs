using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HeroSlidesController : Controller
{
    private readonly ApplicationDbContext _db;

    public HeroSlidesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var list = await _db.HeroSlides.OrderBy(s => s.SortOrder).ToListAsync(ct);
        return View(list);
    }

    public IActionResult Create()
    {
        return View("Edit", new HeroSlide { Id = 0 });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var slide = id == 0 ? new HeroSlide { Id = 0 } : await _db.HeroSlides.FindAsync(id);
        if (slide == null) return NotFound();
        return View(slide);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string Title, string? Text, string? ImageUrl, string? ImageAlt, CancellationToken ct = default)
    {
        HeroSlide slide;
        if (id == 0)
        {
            var order = await _db.HeroSlides.AnyAsync(ct) ? await _db.HeroSlides.MaxAsync(s => s.SortOrder, ct) + 1 : 1;
            slide = new HeroSlide { Title = Title ?? "", Text = Text, ImageUrl = ImageUrl, ImageAlt = ImageAlt, SortOrder = order };
            _db.HeroSlides.Add(slide);
        }
        else
        {
            var existing = await _db.HeroSlides.FindAsync(id);
            if (existing == null) return NotFound();
            slide = existing;
            slide.Title = Title ?? "";
            slide.Text = Text;
            slide.ImageUrl = ImageUrl;
            slide.ImageAlt = ImageAlt;
        }
        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = id == 0 ? "Slayt eklendi." : "Slayt güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(int id, string direction, CancellationToken ct = default)
    {
        var slide = await _db.HeroSlides.FindAsync(id);
        if (slide == null) return RedirectToAction(nameof(Index));
        if (direction == "up")
        {
            var prev = await _db.HeroSlides.Where(s => s.SortOrder < slide.SortOrder).OrderByDescending(s => s.SortOrder).FirstOrDefaultAsync(ct);
            if (prev != null) { (prev.SortOrder, slide.SortOrder) = (slide.SortOrder, prev.SortOrder); await _db.SaveChangesAsync(ct); TempData["Toast"] = "Sıra güncellendi."; }
        }
        else if (direction == "down")
        {
            var next = await _db.HeroSlides.Where(s => s.SortOrder > slide.SortOrder).OrderBy(s => s.SortOrder).FirstOrDefaultAsync(ct);
            if (next != null) { (next.SortOrder, slide.SortOrder) = (slide.SortOrder, next.SortOrder); await _db.SaveChangesAsync(ct); TempData["Toast"] = "Sıra güncellendi."; }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var slide = await _db.HeroSlides.FindAsync(id);
        if (slide != null) { _db.HeroSlides.Remove(slide); await _db.SaveChangesAsync(ct); TempData["Toast"] = "Slayt silindi."; }
        return RedirectToAction(nameof(Index));
    }
}
