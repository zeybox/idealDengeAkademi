using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Services;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly SettingsService _settings;

    public SettingsController(ApplicationDbContext db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var keys = new[] { "SiteName", "SiteDescription", "DefaultCoursePrice", "ContactEmail", "PayTR_MerchantId", "PayTR_MerchantKey", "PayTR_MerchantSalt", "Google_ClientId", "Google_ClientSecret", "AboutPageContent", "MetaDescriptionDefault", "SiteUrl" };
        var dict = new Dictionary<string, string?>();
        foreach (var k in keys) dict[k] = await _settings.GetAsync(k, ct);
        ViewBag.Settings = dict;
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync(ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AboutPage(string? AboutPageContent, CancellationToken ct = default)
    {
        await _settings.SetAsync("AboutPageContent", AboutPageContent, ct);
        TempData["Toast"] = "Hakkımızda sayfası kaydedildi.";
        return RedirectToAction(nameof(Index), new { tab = "panel-hakkimizda" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> General(string? SiteName, string? SiteDescription, string? DefaultCoursePrice, string? ContactEmail, CancellationToken ct = default)
    {
        await _settings.SetAsync("SiteName", SiteName, ct);
        await _settings.SetAsync("SiteDescription", SiteDescription, ct);
        await _settings.SetAsync("DefaultCoursePrice", DefaultCoursePrice, ct);
        await _settings.SetAsync("ContactEmail", ContactEmail, ct);
        TempData["Toast"] = "Genel ayarlar kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SEO(string? SiteName, string? MetaDescriptionDefault, string? SiteUrl, CancellationToken ct = default)
    {
        await _settings.SetAsync("SiteName", SiteName, ct);
        await _settings.SetAsync("MetaDescriptionDefault", MetaDescriptionDefault, ct);
        await _settings.SetAsync("SiteUrl", SiteUrl, ct);
        TempData["Toast"] = "SEO ayarları kaydedildi.";
        return RedirectToAction(nameof(Index), new { tab = "panel-seo" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayTR(string? PayTR_MerchantId, string? PayTR_MerchantKey, string? PayTR_MerchantSalt, CancellationToken ct = default)
    {
        await _settings.SetAsync("PayTR_MerchantId", PayTR_MerchantId, ct);
        await _settings.SetAsync("PayTR_MerchantKey", PayTR_MerchantKey, ct);
        await _settings.SetAsync("PayTR_MerchantSalt", PayTR_MerchantSalt, ct);
        TempData["Toast"] = "PayTR ayarları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Google(string? Google_ClientId, string? Google_ClientSecret, CancellationToken ct = default)
    {
        await _settings.SetAsync("Google_ClientId", Google_ClientId, ct);
        await _settings.SetAsync("Google_ClientSecret", Google_ClientSecret, ct);
        TempData["Toast"] = "Google giriş ayarları kaydedildi. Uygulama yeniden başlatıldığında etkin olur.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryAdd(string Name, CancellationToken ct = default)
    {
        var order = await _db.Categories.AnyAsync(ct) ? await _db.Categories.MaxAsync(c => c.SortOrder, ct) + 1 : 1;
        _db.Categories.Add(new Models.Category { Name = Name, Slug = Slugify(Name), SortOrder = order });
        await _db.SaveChangesAsync(ct);
        TempData["Toast"] = "Kategori eklendi.";
        return RedirectToAction(nameof(Index), new { tab = "panel-kategoriler" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryMove(int id, string direction, CancellationToken ct = default)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return RedirectToAction(nameof(Index), new { tab = "panel-kategoriler" });
        if (direction == "up")
        {
            var prev = await _db.Categories.Where(c => c.SortOrder < cat.SortOrder).OrderByDescending(c => c.SortOrder).FirstOrDefaultAsync(ct);
            if (prev != null) { (prev.SortOrder, cat.SortOrder) = (cat.SortOrder, prev.SortOrder); await _db.SaveChangesAsync(ct); TempData["Toast"] = "Sıra güncellendi."; }
        }
        else if (direction == "down")
        {
            var next = await _db.Categories.Where(c => c.SortOrder > cat.SortOrder).OrderBy(c => c.SortOrder).FirstOrDefaultAsync(ct);
            if (next != null) { (next.SortOrder, cat.SortOrder) = (cat.SortOrder, next.SortOrder); await _db.SaveChangesAsync(ct); TempData["Toast"] = "Sıra güncellendi."; }
        }
        return RedirectToAction(nameof(Index), new { tab = "panel-kategoriler" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryDelete(int id, CancellationToken ct = default)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat != null) { _db.Categories.Remove(cat); await _db.SaveChangesAsync(ct); TempData["Toast"] = "Kategori silindi."; }
        return RedirectToAction(nameof(Index), new { tab = "panel-kategoriler" });
    }

    private static string Slugify(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var t = s.ToLowerInvariant();
        foreach (var (a, b) in new[] { ("ğ","g"),("ü","u"),("ş","s"),("ı","i"),("ö","o"),("ç","c") })
            t = t.Replace(a, b);
        return System.Text.RegularExpressions.Regex.Replace(t, @"[^a-z0-9]+", "-").Trim('-');
    }
}
