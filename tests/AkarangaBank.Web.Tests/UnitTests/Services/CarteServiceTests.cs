using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using Moq;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class CarteServiceTests
{
    [Fact]
    public async Task CarteBloquee_TransactionRefuseeEtHistorisee()
    {
        var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, client, compte, user) = await BankingTestData.SeedAccountAsync(db);
        db.Cartes.Add(CreateCard(client, compte, "Bloquee"));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.AutoriserTransactionAsync(1, user.Id, 1000m, "Paiement");

        Assert.Equal("Refusee", result.Statut);
        Assert.Contains("active", result.MotifRefus!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, db.TransactionsCartes.Count());
        Assert.Empty(db.Operations);
    }

    [Fact]
    public async Task MontantSuperieurAuPlafond_TransactionRefusee()
    {
        var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, client, compte, user) = await BankingTestData.SeedAccountAsync(db);
        db.Cartes.Add(CreateCard(client, compte));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.AutoriserTransactionAsync(1, user.Id, 1000001m, "Paiement");

        Assert.Equal("Refusee", result.Statut);
        Assert.Contains("plafond", result.MotifRefus!, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(db.Operations);
    }

    [Fact]
    public async Task TransactionValide_DebiteEtCreeOperationEtPartieDouble()
    {
        var db = TestDbContextFactory.CreateInMemoryContext();
        var (_, client, compte, user) = await BankingTestData.SeedAccountAsync(db);
        db.Cartes.Add(CreateCard(client, compte));
        await db.SaveChangesAsync();
        var soldeInitial = compte.Solde;
        var service = CreateService(db);

        var result = await service.AutoriserTransactionAsync(1, user.Id, 50000m, "Paiement");

        Assert.Equal("Autorisee", result.Statut);
        Assert.Equal(soldeInitial - 50000m, compte.Solde);
        Assert.Single(db.Operations);
        Assert.Equal(2, db.Ecritures.Count());
        Assert.Equal(50000m, db.Ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant));
        Assert.Equal(50000m, db.Ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant));
        Assert.Equal(result.OperationId, db.Operations.Single().Id);
    }

    private static Carte CreateCard(Client client, Compte compte, string status = "Active") => new()
    {
        Id = 1, NumeroCarte = "**** **** **** 1234", NumeroCarteChiffre = "protected", DerniersChiffres = "1234",
        Compte = compte, CompteId = compte.Id, Client = client, ClientId = client.Id, TypeCarte = "Visa",
        DateExpiration = DateTime.UtcNow.Date.AddYears(1), Statut = status,
        PlafondRetraitJournalier = 500000m, PlafondPaiementJournalier = 1000000m, PlafondEnLigneJournalier = 300000m
    };

    private static CarteService CreateService(AkarangaBank.Web.Data.AkarangaBankContext db)
    {
        var monitoring = new Mock<ISurveillanceService>();
        monitoring.Setup(x => x.AnalyserTransactionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new ResultatSurveillance());
        var protector = new Mock<Microsoft.AspNetCore.DataProtection.IDataProtector>();
        var provider = new Mock<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>();
        provider.Setup(x => x.CreateProtector(It.IsAny<string>())).Returns(protector.Object);
        return new CarteService(db, monitoring.Object, Mock.Of<IDeclarationService>(),
            new CartePanProtection(provider.Object));
    }
}
