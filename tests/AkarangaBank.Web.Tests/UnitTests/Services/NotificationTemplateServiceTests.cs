using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class NotificationTemplateServiceTests
{
    private static NotificationTemplate Modele(int version = 1, string sujet = "Bonjour {{Nom}}") => new()
    {
        Code = "TEST_RENDU", Canal = "Email", Langue = "fr-BI", Sujet = sujet,
        ContenuHtml = "<p>Solde : {{Solde}} pour {{Nom}}</p>",
        ContenuTexte = "Solde : {{Solde}} / {{Inconnue}}",
        Version = version, EstActive = true
    };

    // TU-16.10 : RenderAsync remplace les variables, encode le HTML, laisse intacte une variable inconnue
    [Fact]
    public async Task RenderAsync_RemplaceLesVariablesEtEncodeLeHtml()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        db.NotificationTemplates.Add(Modele());
        await db.SaveChangesAsync();
        var service = new NotificationTemplateService(db);
        var variables = new Dictionary<string, string>
        {
            ["Nom"] = "<b>Alice</b>",
            ["Solde"] = "1500 BIF"
        };

        var rendu = await service.RenderAsync("TEST_RENDU", "Email", "fr-BI", variables);

        Assert.NotNull(rendu);
        Assert.Equal("Bonjour <b>Alice</b>", rendu!.Subject);
        Assert.Equal("<p>Solde : 1500 BIF pour &lt;b&gt;Alice&lt;/b&gt;</p>", rendu.Html);
        Assert.Equal("Solde : 1500 BIF / {{Inconnue}}", rendu.Text);
    }

    [Fact]
    public async Task RenderAsync_ModeleInexistant_RetourneNull()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var service = new NotificationTemplateService(db);

        var rendu = await service.RenderAsync("INCONNU", "Email", "fr-BI", new Dictionary<string, string>());

        Assert.Null(rendu);
    }

    [Fact]
    public async Task RenderAsync_ChoisitLaVersionActiveLaPlusRecente()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        db.NotificationTemplates.Add(Modele(version: 1, sujet: "Ancienne {{Nom}}"));
        db.NotificationTemplates.Add(Modele(version: 2, sujet: "Nouvelle {{Nom}}"));
        await db.SaveChangesAsync();
        var service = new NotificationTemplateService(db);

        var rendu = await service.RenderAsync("TEST_RENDU", "Email", "fr-BI", new Dictionary<string, string> { ["Nom"] = "Alice" });

        Assert.NotNull(rendu);
        Assert.Equal(2, rendu!.Version);
        Assert.Equal("Nouvelle Alice", rendu.Subject);
    }
}
