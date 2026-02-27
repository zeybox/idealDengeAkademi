using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HizliOgren.Services;

namespace HizliOgren.Services;

public class PayTRService
{
    private readonly SettingsService _settings;
    private readonly IHttpContextAccessor _http;
    private static readonly HttpClient HttpClient = new();

    public PayTRService(SettingsService settings, IHttpContextAccessor http)
    {
        _settings = settings;
        _http = http;
    }

    public async Task<string?> GetIframeTokenAsync(string orderId, string email, string fullName, decimal totalLira, string? userAddress, CancellationToken ct = default)
    {
        var merchantId = await _settings.GetAsync("PayTR_MerchantId", ct);
        var merchantKey = await _settings.GetAsync("PayTR_MerchantKey", ct);
        var merchantSalt = await _settings.GetAsync("PayTR_MerchantSalt", ct);
        if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(merchantKey) || string.IsNullOrEmpty(merchantSalt))
            return null;

        var baseUrl = _http.HttpContext?.Request != null
            ? $"{_http.HttpContext.Request.Scheme}://{_http.HttpContext.Request.Host}"
            : "https://www.hizliogren.net.tr";
        var returnUrl = $"{baseUrl}/Checkout/PaySuccess";
        var userIp = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var paymentAmount = (int)(totalLira * 100);
        var userBasket = Convert.ToBase64String(Encoding.UTF8.GetBytes("[]"));

        var hashStr = merchantId + userIp + orderId + email + paymentAmount + userBasket + "0" + "0" + "0" + returnUrl + returnUrl + merchantSalt;
        var tokenBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashStr));
        var paytrToken = Convert.ToBase64String(tokenBytes);

        var form = new Dictionary<string, string>
        {
            ["merchant_id"] = merchantId,
            ["user_ip"] = userIp,
            ["merchant_oid"] = orderId,
            ["email"] = email,
            ["payment_amount"] = paymentAmount.ToString(),
            ["paytr_token"] = paytrToken,
            ["user_basket"] = userBasket,
            ["debug"] = "0",
            ["no_installment"] = "1",
            ["max_installment"] = "0",
            ["user_name"] = fullName,
            ["user_address"] = userAddress ?? "",
            ["user_phone"] = "",
            ["merchant_ok_url"] = returnUrl,
            ["merchant_fail_url"] = returnUrl
        };

        var content = new FormUrlEncodedContent(form);
        var response = await HttpClient.PostAsync("https://www.paytr.com/odeme/api/get-token", content, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("token", out var tok))
                return tok.GetString();
        }
        catch { }
        return null;
    }

    public async Task<bool> VerifyCallbackHashAsync(string merchantOid, string status, string totalAmount, string hash, CancellationToken ct = default)
    {
        var merchantSalt = await _settings.GetAsync("PayTR_MerchantSalt", ct);
        var merchantKey = await _settings.GetAsync("PayTR_MerchantKey", ct);
        if (string.IsNullOrEmpty(merchantSalt) || string.IsNullOrEmpty(merchantKey)) return false;

        var checkStr = merchantOid + merchantSalt + status + totalAmount + merchantKey;
        var expected = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(checkStr)));
        return string.Equals(expected, hash, StringComparison.Ordinal);
    }
}
