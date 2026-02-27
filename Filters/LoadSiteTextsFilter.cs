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
            var favicon = await _settings.GetAsync("FaviconUrl", context.HttpContext.RequestAborted);
            controller.ViewData["FaviconUrl"] = !string.IsNullOrWhiteSpace(favicon) ? favicon.Trim() : "~/images/Logo.png";
            var siteLogo = await _settings.GetAsync("SiteLogoUrl", context.HttpContext.RequestAborted);
            controller.ViewData["SiteLogoUrl"] = !string.IsNullOrWhiteSpace(siteLogo) ? siteLogo.Trim() : "~/images/Logo.png";
            var footerLogo = await _settings.GetAsync("FooterLogoUrl", context.HttpContext.RequestAborted);
            controller.ViewData["FooterLogoUrl"] = !string.IsNullOrWhiteSpace(footerLogo) ? footerLogo.Trim() : null;
            var adminLogo = await _settings.GetAsync("AdminLogoUrl", context.HttpContext.RequestAborted);
            controller.ViewData["AdminLogoUrl"] = !string.IsNullOrWhiteSpace(adminLogo) ? adminLogo.Trim() : null;
        }
        await next();
    }
}
