using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using AkarangaBank.Web.ViewModels;
using Moq;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class VirementServiceTests
{
    [Fact]
    public async Task CreateAsync_RefuseUnMontantNul()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        await BankingTestData.SeedAccountAsync(db);
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new VirementCreateViewModel { CompteSourceId = 1, CompteDestinationId = 2, Montant = 0 }, 1));

        Assert.Empty(db.Virements);
    }

    [Fact]
    public async Task CreateAsync_RefuseUnCompteDestinationIdentique()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        await BankingTestData.SeedAccountAsync(db);
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new VirementCreateViewModel { CompteSourceId = 1, CompteDestinationId = 1, Montant = 100 }, 1));

        Assert.Empty(db.Virements);
    }

    [Fact]
    public async Task CreateAsync_RefuseUnVirementInterbancaireSansBeneficiaire()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        await BankingTestData.SeedAccountAsync(db);
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new VirementCreateViewModel { EstInterbancaire = true, CompteSourceId = 1, Montant = 100 }, 1));

        Assert.Empty(db.Virements);
    }

    [Fact]
    public async Task CreateAsync_CreeVirementInterneEnAttente()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, source, admin) = await BankingTestData.SeedAccountAsync(db);
        var destination = new Compte
        {
            Id = 2, NumeroCompte = "CP-DEST", Client = source.Client, ClientId = source.ClientId,
            TypeCompte = source.TypeCompte, TypeCompteId = source.TypeCompteId, Agence = agence,
            AgenceId = agence.Id, Devise = source.Devise, Solde = 0m, Statut = "Actif"
        };
        db.Comptes.Add(destination);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.CreateAsync(new VirementCreateViewModel
        {
            CompteSourceId = source.Id, CompteDestinationId = destination.Id, Montant = 450m, Motif = "  Loyer  "
        }, admin.Id);

        Assert.Equal("EnAttente", result.Statut);
        Assert.Equal(source.Id, result.CompteSourceId);
        Assert.Equal(destination.Id, result.CompteDestinationId);
        Assert.Equal("Loyer", result.Motif);
        Assert.Equal(source.Devise, result.Devise);
    }

    [Fact]
    public async Task ValiderAsync_VirementEntreAgencesCreeQuatreEcrituresEquilibrees()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agenceSource, _, source, admin) = await BankingTestData.SeedAccountAsync(db, 1000m);
        var agenceDestination = new Agence { Id = 2, Code = "DEST", Nom = "Agence destination", Ville = "Bujumbura" };
        var clientDestination = new Client
        {
            Id = 2, NumeroClient = "CL-DEST", Nom = "Destination", Prenom = "Test",
            DateNaissance = new DateTime(1992, 2, 2), NumeroCNI = "CNI-DEST", Agence = agenceDestination,
            AgenceId = agenceDestination.Id
        };
        var typeCompte = await db.TypesCompte.FindAsync(1) ?? new TypeCompte { Id = 1, Code = "COURANT", Libelle = "Compte courant" };
        var destination = new Compte
        {
            Id = 2, NumeroCompte = "CP-DEST", Client = clientDestination, ClientId = clientDestination.Id,
            TypeCompte = typeCompte, TypeCompteId = typeCompte.Id, Agence = agenceDestination,
            AgenceId = agenceDestination.Id, Devise = "BIF", Solde = 200m, Statut = "Actif"
        };
        db.AddRange(agenceDestination, clientDestination, destination);
        var virement = new Virement
        {
            Id = 1, Reference = "VIR-INTER-AGENCE", CompteSource = source, CompteSourceId = source.Id,
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
        Assert.Equal(4, db.Ecritures.Count());
        Assert.Equal(600m, db.Ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant));
        Assert.Equal(600m, db.Ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant));
        Assert.Equal(2, db.Ecritures.Count(e => e.AgenceId == agenceSource.Id));
        Assert.Equal(2, db.Ecritures.Count(e => e.AgenceId == agenceDestination.Id));
        Assert.Single(db.Operations);
    }

    [Fact]
    public async Task ValiderAsync_SoldeInsuffisantNeDebitePasLeCompte()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, source, admin) = await BankingTestData.SeedAccountAsync(db, 100m);
        var destination = new Compte
        {
            Id = 2, NumeroCompte = "CP-DEST", ClientId = source.ClientId, Client = source.Client,
            TypeCompteId = source.TypeCompteId, TypeCompte = source.TypeCompte, AgenceId = agence.Id,
            Agence = agence, Devise = "BIF", Solde = 0m, Statut = "Actif"
        };
        db.Comptes.Add(destination);
        var virement = new Virement
        {
            Id = 1, Reference = "VIR-SOLDE", CompteSource = source, CompteSourceId = source.Id,
            CompteDestination = destination, CompteDestinationId = destination.Id, Utilisateur = admin,
            UtilisateurId = admin.Id, Montant = 101m, Devise = "BIF", Statut = "EnAttente"
        };
        db.Virements.Add(virement);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValiderAsync(virement.Id, admin.Id));

        Assert.Equal(100m, source.Solde);
        Assert.Equal("EnAttente", virement.Statut);
        Assert.Empty(db.Ecritures);
        Assert.Empty(db.Operations);
    }

    [Fact]
    public async Task RejeterAsync_MarqueLeVirementEnAttenteCommeRejete()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, source, admin) = await BankingTestData.SeedAccountAsync(db);
        var virement = new Virement
        {
            Id = 1, Reference = "VIR-REJET", CompteSource = source, CompteSourceId = source.Id,
            Utilisateur = admin, UtilisateurId = admin.Id, Montant = 100m, Devise = source.Devise,
            Statut = "EnAttente"
        };
        db.Virements.Add(virement);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.RejeterAsync(virement.Id, admin.Id);

        Assert.True(result);
        Assert.Equal("Rejete", virement.Statut);
        Assert.NotNull(virement.DateValidation);
        Assert.Equal(agence.Id, source.AgenceId);
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
