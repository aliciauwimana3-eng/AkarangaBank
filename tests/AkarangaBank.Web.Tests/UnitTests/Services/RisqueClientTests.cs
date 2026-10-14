using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class RisqueClientTests
{
    // TU-16.8 : client avec alerte AML -> risque eleve
    [Fact]
    public async Task EvaluerRisqueClientAsync_ClientAvecAlerte_RetourneEleve()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, client, _, _) = await BankingTestData.SeedAccountAsync(db);
        db.AlertesAML.Add(new AlerteAML { Client = client, ClientId = client.Id, TypeAlerte = "DepassementSeuil" });
        await db.SaveChangesAsync();
        var service = new ConformiteService(db);

        var risque = await service.EvaluerRisqueClientAsync(client.Id);

        Assert.Equal(NiveauRisqueClient.Eleve, risque);
    }

    [Fact]
    public async Task EvaluerRisqueClientAsync_ClientSansHistorique_RetourneFaible()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, client, _, _) = await BankingTestData.SeedAccountAsync(db);
        var service = new ConformiteService(db);

        var risque = await service.EvaluerRisqueClientAsync(client.Id);

        Assert.Equal(NiveauRisqueClient.Faible, risque);
    }

    [Fact]
    public async Task EvaluerRisqueClientAsync_ClientInexistant_RetourneInconnu()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var service = new ConformiteService(db);

        var risque = await service.EvaluerRisqueClientAsync(9999);

        Assert.Equal(NiveauRisqueClient.Inconnu, risque);
    }
}
