using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;
using System.Security.Claims;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BlogController : Controller
{
    private readonly ApplicationDbContext _db;

    public BlogController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, CancellationToken ct = default)
    {
        var query = _db.BlogPosts.Include(b => b.Author).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(b => b.Title.Contains(q) || (b.Excerpt != null && b.Excerpt.Contains(q)));
        var list = await query.OrderByDescending(b => b.CreatedAt).ToListAsync(ct);
        ViewBag.Q = q;
        return View(list);
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken ct = default)
    {
        if (!id.HasValue) return View(new BlogPost { IsPublished = false });
        var post = await _db.BlogPosts.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (post == null) return NotFound();
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BlogPost model, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (model.Id == 0)
        {
            model.AuthorId = userId;
            model.CreatedAt = DateTime.UtcNow;
            model.Slug = Slugify(model.Title);
            model.Content = EnsureLinksBlank(model.Content);
            _db.BlogPosts.Add(model);
            await _db.SaveChangesAsync(ct);
            TempData["Toast"] = "Yazı eklendi.";
            return RedirectToAction(nameof(Index));
        }
        var post = await _db.BlogPosts.FirstOrDefaultAsync(b => b.Id == model.Id, ct);
        if (post == null) return NotFound();
        post.Title = model.Title;
        post.Slug = Slugify(model.Title);
        post.Excerpt = model.Excerpt;
        post.Content = EnsureLinksBlank(model.Content);
        post.ImageUrl = model.ImageUrl;
        post.IsPublished = model.IsPublished;
        if (model.IsPublished && !post.PublishedAt.HasValue) post.PublishedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = "Yazı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var post = await _db.BlogPosts.FindAsync(id);
        if (post == null) return NotFound();
        _db.BlogPosts.Remove(post);
        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = "Yazı silindi.";
        return RedirectToAction(nameof(Index));
    }

    private static string Slugify(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var t = s.ToLowerInvariant();
        foreach (var (a, b) in new[] { ("ğ","g"),("ü","u"),("ş","s"),("ı","i"),("ö","o"),("ç","c") })
            t = t.Replace(a, b);
        return Regex.Replace(t, @"[^a-z0-9]+", "-").Trim('-');
    }

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
}
