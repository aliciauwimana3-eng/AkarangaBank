using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Controllers;

public class CultureController : Controller
{
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        if (!LocalizationService.SupportedCultures.Any(c => c.Name.Equals(culture, StringComparison.OrdinalIgnoreCase)))
            return BadRequest("Culture non supportée.");

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps });
        var localReturn = !string.IsNullOrEmpty(returnUrl) && returnUrl[0] == '/' &&
            !returnUrl.StartsWith("//", StringComparison.Ordinal) && !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
        return localReturn ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home")!;
    }
}
