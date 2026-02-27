using HizliOgren.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HizliOgren.Filters;

public class LoadSiteTextsFilter : IAsyncResultFilter
{
    private readonly SettingsService _settings;

    public LoadSiteTextsFilter(SettingsService settings) => _settings = settings;

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Controller is Controller controller)
        {
            var dict = await _settings.GetManyAsync(SiteTextKeys.AllKeys, context.HttpContext.RequestAborted);
            foreach (var key in SiteTextKeys.AllKeys)
            {
                controller.ViewData[key] = dict.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v)
                    ? v
                    : SiteTextKeys.GetDefault(key);
            }
            controller.ViewData["SiteName"] = await _settings.GetAsync("SiteName", context.HttpContext.RequestAborted)
                ?? "İdeal Denge Akademi";
            var siteUrl = await _settings.GetAsync("SiteUrl", context.HttpContext.RequestAborted);
            controller.ViewData["SiteUrl"] = !string.IsNullOrWhiteSpace(siteUrl) ? siteUrl.Trim() : null;
        }
        await next();
    }
}
