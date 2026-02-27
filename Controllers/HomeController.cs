using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;

namespace HizliOgren.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var featured = await _db.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.CurriculumItems)
            .Where(c => c.IsPublished)
            .OrderByDescending(c => c.CreatedAt)
            .Take(8)
            .AsNoTracking()
            .ToListAsync(ct);
        ViewBag.FeaturedCourses = featured;
        var slides = await _db.HeroSlides.OrderBy(s => s.SortOrder).AsNoTracking().ToListAsync(ct);
        ViewBag.HeroSlides = slides;
        var latestPosts = await _db.BlogPosts
            .Include(b => b.Author)
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Take(6)
            .AsNoTracking()
            .ToListAsync(ct);
        ViewBag.LatestBlogPosts = latestPosts;
        return View();
    }

    public IActionResult Hakkimizda()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
