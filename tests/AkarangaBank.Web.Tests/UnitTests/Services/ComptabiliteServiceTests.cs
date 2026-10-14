using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class ComptabiliteServiceTests
{
    [Fact]
    public async Task VerifierEquilibreGlobal_EcrituresEquilibrees_RetourneVrai()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        var operation = BankingTestData.CreateOperation(compte, utilisateur, agence);
        var compteGeneral = new CompteComptable { Id = 1, Code = "CAISSE-BIF", Libelle = "Caisse BIF", Devise = "BIF" };
        db.Operations.Add(operation);
        db.ComptesComptables.Add(compteGeneral);
        db.Ecritures.AddRange(
            new Ecriture { Operation = operation, Compte = compte, Sens = "Debit", Montant = 100_000m, Devise = "BIF" },
            new Ecriture { Operation = operation, CompteComptable = compteGeneral, Sens = "Credit", Montant = 100_000m, Devise = "BIF" });
        await db.SaveChangesAsync();

        Assert.True(await new ComptabiliteService(db).VerifierEquilibreGlobalAsync());
    }

    [Fact]
    public async Task VerifierEquilibreGlobal_EcritureDesequilibree_RetourneFaux()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        var operation = BankingTestData.CreateOperation(compte, utilisateur, agence);
        db.Operations.Add(operation);
        db.Ecritures.Add(new Ecriture { Operation = operation, Compte = compte, Sens = "Debit", Montant = 100_000m, Devise = "BIF" });
        await db.SaveChangesAsync();

        Assert.False(await new ComptabiliteService(db).VerifierEquilibreGlobalAsync());
    }
}
