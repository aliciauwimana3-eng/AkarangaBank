using System.Globalization;
using AkarangaBank.Web.Services;
using AkarangaBank.Web;
using Microsoft.Extensions.Localization;
using Moq;

namespace AkarangaBank.Web.Tests.UnitTests;

public class LocalizationServiceTests
{
    [Theory]
    [InlineData("fr-BI", "15/10/2026", "1\u202F500\u202F000,00 BIF")]
    [InlineData("en-US", "10/15/2026", "1,500,000.00 BIF")]
    public void DateEtMontant_SuiventLaCulture(string cultureName, string expectedDate, string expectedAmount)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            var service = new LocalizationService(Mock.Of<IStringLocalizer<SharedResource>>());

            Assert.Equal(expectedDate, service.FormaterDate(new DateTime(2026, 10, 15)));
            Assert.Equal(expectedAmount, service.FormaterMontant(1500000m));
        }
        finally { CultureInfo.CurrentCulture = previousCulture; }
    }
}
