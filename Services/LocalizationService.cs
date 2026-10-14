using System.Globalization;
using Microsoft.Extensions.Localization;

namespace AkarangaBank.Web.Services;

public class LocalizationService(IStringLocalizer<SharedResource> localizer)
{
    public static IReadOnlyList<CultureInfo> SupportedCultures { get; } =
    [CultureInfo.GetCultureInfo("fr-BI"), CultureInfo.GetCultureInfo("rn-BI"),
     CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("sw-KE")];

    public LocalizedString this[string key] => localizer[key];
    public string FormaterDate(DateTime date) => date.ToString("d", CultureInfo.CurrentCulture);
    public string FormaterMontant(decimal montant, string devise = "BIF") =>
        string.Format(CultureInfo.CurrentCulture, "{0:N2} {1}", montant, devise);
}
