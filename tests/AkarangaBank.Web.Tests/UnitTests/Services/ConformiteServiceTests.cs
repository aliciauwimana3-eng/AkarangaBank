using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class ConformiteServiceTests
{
    [Fact]
    public async Task AnalyserTransaction_AuSeuilAML_ExigeUneVigilanceRenforcee()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, _, compte, _) = await BankingTestData.SeedAccountAsync(db);
        var service = new ConformiteService(db);

        var resultat = await service.AnalyserTransactionAsync(compte.Id, 5_000_000m, "Depot");

        Assert.True(resultat.NecessiteVigilanceRenforcee);
        Assert.False(resultat.EstBloquee);
    }

    [Fact]
    public async Task AnalyserTransaction_DepotAuSeuilDeclaration_DemandeUneDeclaration()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, _, compte, _) = await BankingTestData.SeedAccountAsync(db);
        var service = new ConformiteService(db);

        var resultat = await service.AnalyserTransactionAsync(compte.Id, 10_000_000m, "Depot");

        Assert.True(resultat.NecessiteDeclaration);
    }

    [Fact]
    public async Task DetecterFractionnement_SommeDesOperationsSousLeSeuilSur24Heures()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        var date = DateTime.UtcNow;
        var operation = BankingTestData.CreateOperation(compte, utilisateur, agence);
        operation.DateOperation = date.AddHours(-2);
        db.Operations.Add(operation);
        db.Ecritures.Add(new AkarangaBank.Web.Models.Ecriture
        {
            Operation = operation, Compte = compte, Sens = "Credit", Montant = 3_000_000m,
            Devise = "BIF", DateEcriture = date.AddHours(-2)
        });
        await db.SaveChangesAsync();
        var service = new ConformiteService(db);

        var fractionnement = await service.DetecterFractionnementAsync(compte.Id, 2_100_000m, date);

        Assert.True(fractionnement);
    }
}
