using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using Moq;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class SurveillanceServiceExtendedTests
{
    [Fact]
    public async Task AnalyserTransaction_FrequenceEtMontantInhabituelProduisentDesAlertes()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        var date = DateTime.UtcNow;
        for (var i = 0; i < 21; i++)
        {
            var operation = BankingTestData.CreateOperation(compte, utilisateur, agence, $"OPE-HIST-{i}");
            operation.DateOperation = date.AddHours(-i);
            operation.Montant = 100m;
            db.Operations.Add(operation);
            db.Ecritures.Add(new Ecriture
            {
                Operation = operation, Compte = compte, Sens = "Credit", Montant = 100m,
                Devise = "BIF", DateEcriture = operation.DateOperation
            });
        }
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.AnalyserTransactionAsync(compte.Id, 1000m, "Depot", date);

        Assert.Contains(result.Alertes, alert => alert.Regle == "FrequenceElevee");
        Assert.Contains(result.Alertes, alert => alert.Regle == "MontantInhabituel");
        Assert.True(result.NecessiteDeclaration);
    }

    [Fact]
    public async Task AnalyserTransaction_DepotsSurTroisComptesDeclenchentSmurfing()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, client, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        var compteDeux = CreateSiblingAccount(2, client, compte);
        var compteTrois = CreateSiblingAccount(3, client, compte);
        db.Comptes.AddRange(compteDeux, compteTrois);
        var date = DateTime.UtcNow;
        AddRecentDeposit(db, compteDeux, utilisateur, agence, date, "OPE-SMURF-2", 2_000_000m);
        AddRecentDeposit(db, compteTrois, utilisateur, agence, date, "OPE-SMURF-3", 2_000_000m);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.AnalyserTransactionAsync(compte.Id, 1_000_000m, "Depot", date);

        Assert.Contains(result.Alertes, alert => alert.Regle == "Smurfing");
        Assert.True(result.NecessiteDeclaration);
    }

    [Fact]
    public async Task AnalyserTransaction_VirementsRapidesDeclenchentLayering()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, _, compte, utilisateur) = await BankingTestData.SeedAccountAsync(db);
        var date = DateTime.UtcNow;
        for (var i = 0; i < 3; i++)
        {
            db.Virements.Add(new Virement
            {
                Reference = $"VIR-LAYER-{i}", CompteSource = compte, CompteSourceId = compte.Id,
                Utilisateur = utilisateur, UtilisateurId = utilisateur.Id, Montant = 2_000_000m,
                Devise = "BIF", Statut = "Valide", DateVirement = date.AddMinutes(-i)
            });
        }
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.AnalyserTransactionAsync(compte.Id, 100m, "Virement", date);

        Assert.Contains(result.Alertes, alert => alert.Regle == "Layering");
        Assert.True(result.NecessiteDeclaration);
    }

    [Fact]
    public async Task AnalyserTransaction_CompteInexistantRetourneUnResultatVide()
    {
        await using var db = TestDbContextFactory.CreateInMemoryContext();
        var service = CreateService(db);
        var date = DateTime.UtcNow;

        var result = await service.AnalyserTransactionAsync(999, 500m, "Depot", date);

        Assert.Equal(999, result.CompteId);
        Assert.Equal(date, result.DateAnalyse);
        Assert.Empty(result.Alertes);
        Assert.False(result.NecessiteDeclaration);
    }

    private static Compte CreateSiblingAccount(int id, Client client, Compte original) => new()
    {
        Id = id, NumeroCompte = $"CP-TEST-{id}", Client = client, ClientId = client.Id,
        TypeCompte = original.TypeCompte, TypeCompteId = original.TypeCompteId,
        Agence = original.Agence, AgenceId = original.AgenceId, Devise = original.Devise,
        Solde = 0m, Statut = "Actif"
    };

    private static void AddRecentDeposit(AkarangaBankContext db, Compte compte,
        ApplicationUser utilisateur, Agence agence, DateTime date, string reference, decimal montant)
    {
        var operation = BankingTestData.CreateOperation(compte, utilisateur, agence, reference);
        operation.DateOperation = date.AddMinutes(-2);
        operation.Montant = montant;
        db.Operations.Add(operation);
        db.Ecritures.Add(new Ecriture
        {
            Operation = operation, Compte = compte, Sens = "Credit", Montant = montant,
            Devise = "BIF", DateEcriture = operation.DateOperation
        });
    }

    private static SurveillanceService CreateService(AkarangaBankContext db)
    {
        var conformite = new Mock<IConformiteService>();
        conformite.Setup(x => x.GetSeuilAMLAsync(It.IsAny<int?>())).ReturnsAsync(5_000_000m);
        conformite.Setup(x => x.EvaluerRisqueClientAsync(It.IsAny<int>())).ReturnsAsync(NiveauRisqueClient.Faible);
        conformite.Setup(x => x.DetecterFractionnementAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        return new SurveillanceService(db, conformite.Object);
    }
}
