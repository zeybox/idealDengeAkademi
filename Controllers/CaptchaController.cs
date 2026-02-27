using Microsoft.AspNetCore.Mvc;
using HizliOgren.Services;

namespace HizliOgren.Controllers;

/// <summary>CAPTCHA resmi döner; cevap session'da saklanır (key: Captcha_Register veya Captcha_Basvuru).</summary>
public class CaptchaController : Controller
{
    private const string SessionKeyPrefix = "Captcha_";
    private readonly CaptchaService _captcha;

    public CaptchaController(CaptchaService captcha) => _captcha = captcha;

    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true, Duration = 0)]
    public IActionResult Image(string key = "Register")
    {
        var (code, imageBytes) = _captcha.Generate();
        var sessionKey = SessionKeyPrefix + (string.IsNullOrWhiteSpace(key) ? "Register" : key);
        HttpContext.Session.SetString(sessionKey, code);
        return File(imageBytes, "image/png");
    }
}
