using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;

namespace HizliOgren.Controllers;

public class BlogController : Controller
{
    private readonly ApplicationDbContext _db;

    public BlogController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, CancellationToken ct = default)
    {
        var query = _db.BlogPosts
            .Include(b => b.Author)
            .Where(b => b.IsPublished)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(b => b.Title.Contains(q) || (b.Excerpt != null && b.Excerpt.Contains(q)));

        var list = await query.OrderByDescending(b => b.PublishedAt ?? b.CreatedAt).ToListAsync(ct);
        ViewBag.Q = q;
        return View(list);
    }

    public async Task<IActionResult> Post(string slug, CancellationToken ct = default)
    {
        var post = await _db.BlogPosts
            .Include(b => b.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished, ct);
        if (post == null) return NotFound();
        return View(post);
    }
}
