using AkarangaBank.Web.Services;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using Moq;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class SurveillanceServiceTests
{
    [Fact]
    public async Task AnalyserTransaction_SousLesSeuils_SansHistoriqueNeDeclenchePasDalerte()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, client, compte, _) = await BankingTestData.SeedAccountAsync(db);
        var conformite = new Mock<IConformiteService>(MockBehavior.Strict);
        conformite.Setup(s => s.GetSeuilAMLAsync(agence.Id)).ReturnsAsync(5_000_000m);
        conformite.Setup(s => s.EvaluerRisqueClientAsync(client.Id)).ReturnsAsync(NiveauRisqueClient.Faible);
        conformite.Setup(s => s.DetecterFractionnementAsync(compte.Id, 100_000m, It.IsAny<DateTime>())).ReturnsAsync(false);

        var resultat = await new SurveillanceService(db, conformite.Object)
            .AnalyserTransactionAsync(compte.Id, 100_000m, "Depot", DateTime.UtcNow);

        Assert.Empty(resultat.Alertes);
        Assert.False(resultat.NecessiteDeclaration);
        conformite.VerifyAll();
    }
}
