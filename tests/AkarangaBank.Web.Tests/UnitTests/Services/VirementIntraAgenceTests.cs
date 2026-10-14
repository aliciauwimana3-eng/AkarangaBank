using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using AkarangaBank.Web.ViewModels;
using Moq;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class VirementIntraAgenceTests
{
    // TU-16.3 : un virement entre deux comptes de la MEME agence cree deux ecritures equilibrees
    [Fact]
    public async Task ValiderAsync_VirementIntraAgenceCreeDeuxEcrituresEquilibrees()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, source, admin) = await BankingTestData.SeedAccountAsync(db, 1000m);
        var destination = new Compte
        {
            Id = 2, NumeroCompte = "CP-DEST", Client = source.Client, ClientId = source.ClientId,
            TypeCompte = source.TypeCompte, TypeCompteId = source.TypeCompteId, Agence = agence,
            AgenceId = agence.Id, Devise = "BIF", Solde = 200m, Statut = "Actif"
        };
        db.Comptes.Add(destination);
        var virement = new Virement
        {
            Id = 1, Reference = "VIR-INTRA", CompteSource = source, CompteSourceId = source.Id,
            CompteDestination = destination, CompteDestinationId = destination.Id, Utilisateur = admin,
            UtilisateurId = admin.Id, Montant = 300m, Devise = "BIF", Statut = "EnAttente"
        };
        db.Virements.Add(virement);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.ValiderAsync(virement.Id, admin.Id);

        Assert.True(result);
        Assert.Equal(700m, source.Solde);
        Assert.Equal(500m, destination.Solde);
        Assert.Equal("Valide", virement.Statut);
        Assert.Equal(2, db.Ecritures.Count());
        Assert.Equal(300m, db.Ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant));
        Assert.Equal(300m, db.Ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant));
    }

    private static VirementService CreateService(AkarangaBank.Web.Data.AkarangaBankContext db)
    {
        var conformite = new Mock<IConformiteService>();
        conformite.Setup(x => x.AnalyserTransactionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new ResultatConformite());
        conformite.Setup(x => x.DetecterFractionnementAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        var surveillance = new Mock<ISurveillanceService>();
        surveillance.Setup(x => x.AnalyserTransactionAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new ResultatSurveillance());
        return new VirementService(db, conformite.Object, surveillance.Object, Mock.Of<IDeclarationService>());
    }
}
