using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HizliOgren.Services;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SiteTextsController : Controller
{
    private readonly SettingsService _settings;

    public SiteTextsController(SettingsService settings) => _settings = settings;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var dict = await _settings.GetManyAsync(SiteTextKeys.AllKeys, ct);
        ViewBag.Texts = dict;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IFormCollection form, CancellationToken ct = default)
    {
        foreach (var key in SiteTextKeys.FormKeys)
        {
            var value = form[key].ToString();
            await _settings.SetAsync(key, value ?? "", ct);
        }
        TempData["Toast"] = "Site sabit yazıları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }
}
