using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class ReferenceGeneratorTests
{
    // TU-16.9 : format PREFIXE-AAAAMMJJ-XXXXXX
    [Fact]
    public void Generer_RespecteLeFormatPrefixeDateEtSixCaracteres()
    {
        var reference = ReferenceGenerator.Generer("AKR");

        Assert.Matches($"^AKR-{DateTime.UtcNow:yyyyMMdd}-[0-9A-F]{{6}}$", reference);
    }

    [Fact]
    public void Generer_ProduitDesReferencesDistinctes()
    {
        Assert.NotEqual(ReferenceGenerator.Generer("AKR"), ReferenceGenerator.Generer("AKR"));
    }
}
