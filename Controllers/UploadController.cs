using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace HizliOgren.Controllers;

[Authorize]
public class UploadController : Controller
{
    /// <summary>Uygulama genelinde hiçbir dosya yüklemesi bu değeri aşmamalı (Kestrel limiti ile uyumlu).</summary>
    private const long MaxFileSizeBytes = 30 * 1024 * 1024; // 30 MB
    private const int MaxImageWidthPx = 1280;

    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedImageContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public UploadController(IWebHostEnvironment env) => _env = env;

    /// <summary>Öğrenci sadece resim yükleyebilir; Admin/Egitmen tüm resim formatlarını yükleyebilir. Maks. 5 MB (genel üst sınır 30 MB).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<IActionResult> Image(IFormFile? upload, CancellationToken ct = default)
    {
        if (upload == null || upload.Length == 0)
            return Json(new { error = new { message = "Dosya seçilmedi." } });
        if (upload.Length > MaxFileSizeBytes)
            return Json(new { error = new { message = "Dosya boyutu 30 MB sınırını aşamaz." } });

        var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext))
            return Json(new { error = new { message = "İzin verilen formatlar: JPG, PNG, GIF, WEBP." } });

        // Öğrenci yetkisindekiler sadece resim yükleyebilir; Content-Type ile de doğrula (taklit uzantı engeli)
        if (!User.IsInRole("Admin") && !User.IsInRole("Egitmen"))
        {
            var contentType = (upload.ContentType ?? "").Split(';')[0].Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType) || !AllowedImageContentTypes.Contains(contentType))
                return Json(new { error = new { message = "Sadece resim dosyaları (JPG, PNG, GIF, WEBP) yükleyebilirsiniz." } });
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        try
        {
            await using var inputStream = upload.OpenReadStream();
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream, ct);
            if (image.Width > MaxImageWidthPx)
                image.Mutate(x => x.Resize(MaxImageWidthPx, 0)); // En-boy oranı korunur (height 0 = otomatik)
            image.Save(filePath); // Uzantıya göre format seçilir; PNG şeffaflık korunur
        }
        catch (SixLabors.ImageSharp.UnknownImageFormatException)
        {
            return Json(new { error = new { message = "Geçerli bir resim dosyası değil." } });
        }
        catch (Exception)
        {
            return Json(new { error = new { message = "Resim işlenirken hata oluştu." } });
        }

        var url = Url.Content($"~/uploads/{fileName}");
        return Json(new { url });
    }

    /// <summary>Sadece Admin ve Eğitmen PDF yükleyebilir; öğrenci kesinlikle yasak. Maks. 15 MB (genel üst sınır 30 MB).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(15 * 1024 * 1024)] // 15 MB
    [Authorize(Roles = "Admin,Egitmen")]
    public async Task<IActionResult> Pdf(IFormFile? file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return Json(new { error = new { message = "Dosya seçilmedi." } });
        if (file.Length > MaxFileSizeBytes)
            return Json(new { error = new { message = "Dosya boyutu 30 MB sınırını aşamaz." } });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf")
            return Json(new { error = new { message = "Sadece PDF dosyaları yüklenebilir." } });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}.pdf";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream, ct);

        var url = Url.Content($"~/uploads/{fileName}");
        return Json(new { url, name = Path.GetFileNameWithoutExtension(file.FileName) });
    }
}
