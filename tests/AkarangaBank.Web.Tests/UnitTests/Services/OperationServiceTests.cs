using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using AkarangaBank.Web.ViewModels;
using Moq;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class OperationServiceTests
{
    [Theory]
    [InlineData(0, "Depot")]
    [InlineData(-1, "Retrait")]
    [InlineData(100, "Virement")]
    public async Task CreateAsync_RefuseMontantOuTypeInvalide(decimal montant, string type)
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new OperationCreateViewModel { CompteId = 1, Montant = montant, TypeOperation = type }, 1));

        Assert.Empty(db.Operations);
    }

    [Fact]
    public async Task CreateAsync_CreeOperationEnAttentePourCompteClient()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, _, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        utilisateur.Role = "client";
        utilisateur.ClientId = compte.ClientId;
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var operation = await service.CreateAsync(new OperationCreateViewModel
        {
            CompteId = compte.Id, TypeOperation = "Depot", Montant = 25000m, Commentaire = "  Versement  "
        }, utilisateur.Id);

        Assert.Equal("EnAttente", operation.Statut);
        Assert.Equal(compte.AgenceId, operation.AgenceId);
        Assert.Equal("Versement", operation.Commentaire);
        Assert.Equal(25000m, operation.Montant);
    }

    [Fact]
    public async Task CreateAsync_RefuseLeCompteDUnAutreClient()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, _, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        utilisateur.Role = "client";
        utilisateur.ClientId = 999;
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new OperationCreateViewModel { CompteId = compte.Id, Montant = 100m, TypeOperation = "Depot" }, utilisateur.Id));

        Assert.Empty(db.Operations);
    }

    [Fact]
    public async Task ValiderAsync_DepotCrediteCompteEtCreeDeuxEcrituresEquilibrees()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, admin) = await BankingTestData.SeedAccountAsync(db, 1000m);
        var operation = BankingTestData.CreateOperation(compte, admin, agence, "OP-DEPOT");
        operation.Statut = "EnAttente";
        operation.Montant = 750m;
        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.ValiderAsync(operation.Id, admin.Id);

        Assert.True(result);
        Assert.Equal(1750m, compte.Solde);
        Assert.Equal("Valide", operation.Statut);
        Assert.Equal(2, db.Ecritures.Count());
        Assert.Equal(750m, db.Ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant));
        Assert.Equal(750m, db.Ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant));
        Assert.All(db.Ecritures, e => Assert.Equal(compte.AgenceId, e.AgenceId));
    }

    [Fact]
    public async Task ValiderAsync_RetraitSansSoldeSuffisantNeModifieRien()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, admin) = await BankingTestData.SeedAccountAsync(db, 100m);
        var operation = BankingTestData.CreateOperation(compte, admin, agence, "OP-RETRAIT");
        operation.TypeOperation = "Retrait";
        operation.Statut = "EnAttente";
        operation.Montant = 101m;
        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValiderAsync(operation.Id, admin.Id));

        Assert.Equal(100m, compte.Solde);
        Assert.Equal("EnAttente", operation.Statut);
        Assert.Empty(db.Ecritures);
    }

    [Fact]
    public async Task ValiderAsync_OperationBloqueeNeCreeAucuneEcriture()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, admin) = await BankingTestData.SeedAccountAsync(db);
        var operation = BankingTestData.CreateOperation(compte, admin, agence, "OP-BLOQUE");
        operation.Statut = "EnAttente";
        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        var conformite = new Mock<IConformiteService>();
        conformite.Setup(x => x.AnalyserTransactionAsync(compte.Id, operation.Montant, operation.TypeOperation))
            .ReturnsAsync(new ResultatConformite { EstBloquee = true, Motif = "Seuil de blocage" });
        var service = CreateService(db, conformite.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValiderAsync(operation.Id, admin.Id));

        Assert.Contains("Seuil de blocage", exception.Message);
        Assert.Equal("EnAttente", operation.Statut);
        Assert.Empty(db.Ecritures);
    }

    private static OperationService CreateService(AkarangaBank.Web.Data.AkarangaBankContext db,
        IConformiteService? conformiteService = null)
    {
        var conformite = new Mock<IConformiteService>();
        conformite.Setup(x => x.AnalyserTransactionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new ResultatConformite());
        conformite.Setup(x => x.DetecterFractionnementAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        var surveillance = new Mock<ISurveillanceService>();
        surveillance.Setup(x => x.AnalyserTransactionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new ResultatSurveillance());
        return new OperationService(db, conformiteService ?? conformite.Object, surveillance.Object,
            Mock.Of<IDeclarationService>());
    }
}
