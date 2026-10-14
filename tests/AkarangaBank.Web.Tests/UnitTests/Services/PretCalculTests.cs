using AkarangaBank.Web.Services;
using AkarangaBank.Web.Tests.UnitTests.Helpers;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class PretCalculTests
{
    [Fact]
    public void Mensualite_SansInteret_EgaleCapitalDiviseParDuree()
        => Assert.Equal(100000m, PretService.CalculerMensualite(1200000m, 0m, 12));

    [Fact]
    public void Mensualite_Avec12Pourcent_EstAmortissable()
        => Assert.InRange(PretService.CalculerMensualite(1000000m, 12m, 12), 88848m, 88850m);

    [Theory]
    [InlineData(1000000, 0, 10, 100000)]
    [InlineData(1200000, 0, 12, 100000)]
    [InlineData(500000, 0, 5, 100000)]
    public void Mensualite_AZeroPourcent_EstExacte(decimal capital, decimal taux, int duree, decimal attendu)
        => Assert.Equal(attendu, PretService.CalculerMensualite(capital, taux, duree));

    [Fact]
    public void Echeancier_SommeDuCapitalEtSoldeFinalSontExacts()
    {
        var echeances = PretService.GenererEcheancier(1000000m, 12m, 12,
            PretService.CalculerMensualite(1000000m, 12m, 12), new DateTime(2026, 1, 15));

        Assert.Equal(1000000m, echeances.Sum(e => e.Capital));
        Assert.Equal(0m, 1000000m - echeances.Sum(e => e.Capital));
        Assert.Equal(12, echeances.Count);
        Assert.Equal(new DateTime(2027, 1, 15), echeances[^1].DateEcheance);
    }

    [Theory]
    [InlineData("Visa", '4')]
    [InlineData("MasterCard", '5')]
    [InlineData("Locale", '6')]
    public void NumeroCarte_GenereUnPanDe16ChiffresAvecPrefixeEtLuhn(string type, char prefixe)
    {
        var pan = CarteService.GenererNumero(type);
        Assert.Equal(16, pan.Length);
        Assert.All(pan, c => Assert.True(char.IsDigit(c)));
        Assert.Equal(prefixe, pan[0]);
        var somme = 0;
        var doubler = false;
        for (var i = pan.Length - 1; i >= 0; i--)
        {
            var chiffre = pan[i] - '0';
            if (doubler && (chiffre *= 2) > 9) chiffre -= 9;
            somme += chiffre;
            doubler = !doubler;
        }
        Assert.Equal(0, somme % 10);
    }

    [Fact]
    public async Task Debloquer_CrediteCompteEtCreeLesEcritures()
    {
        var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, client, compte, user) = await BankingTestData.SeedAccountAsync(db);
        var pret = await SeedPretAsync(db, agence, client, compte);
        var balance = compte.Solde;

        Assert.True(await new PretService(db).TraiterAsync(pret.Id, user.Id, true));

        Assert.Equal(balance + pret.MontantPrincipal, compte.Solde);
        Assert.Equal("Valide", pret.Statut);
        Assert.Equal(12, db.EcheancesPret.Count());
        Assert.Equal(2, db.Ecritures.Count());
    }

    [Fact]
    public async Task Rembourser_DebiteLeCompteEtSoldeLePret()
    {
        var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, client, compte, user) = await BankingTestData.SeedAccountAsync(db);
        var pret = await SeedPretAsync(db, agence, client, compte, duree: 1);
        pret.Statut = "Impay\u00E9";
        foreach (var e in PretService.GenererEcheancier(120000m, 0m, 1, 120000m, DateTime.UtcNow)) pret.Echeances.Add(e);
        var echeance = pret.Echeances.Single();
        echeance.Statut = "EnRetard";
        echeance.Penalite = 500m;
        await db.SaveChangesAsync();
        var balance = compte.Solde;

        Assert.True(await new PretService(db).PayerEcheanceAsync(1, user.Id));

        Assert.Equal(balance - 120500m, compte.Solde);
        Assert.Equal("Payee", db.EcheancesPret.Single().Statut);
        Assert.Equal("Solde", pret.Statut);
        Assert.Equal(120500m, db.Operations.Single().Montant);
    }

    [Fact]
    public async Task MettreAJourRetards_PenaliseEtPassePretEnImpayeApresTroisEcheances()
    {
        var db = TestDbContextFactory.CreateInMemoryContext();
        var (agence, client, compte, _) = await BankingTestData.SeedAccountAsync(db);
        var pret = await SeedPretAsync(db, agence, client, compte, duree: 3);
        pret.Statut = "Valide";
        foreach (var e in Enumerable.Range(1, 3).Select(n => new EcheancePret
        {
            NumeroEcheance = n, Capital = 100000m, Interet = 1000m, MontantTotal = 101000m,
            DateEcheance = DateTime.UtcNow.Date.AddMonths(-n), Statut = "EnAttente"
        })) pret.Echeances.Add(e);
        await db.SaveChangesAsync();

        var count = await new PretService(db).MettreAJourRetardsAsync(DateTime.UtcNow.Date);

        Assert.Equal(3, count);
        Assert.All(db.EcheancesPret, e => Assert.Equal("EnRetard", e.Statut));
        Assert.All(db.EcheancesPret, e => Assert.True(e.Penalite >= 500m));
        Assert.Equal("Impayé", db.Prets.Single().Statut);
    }

    private static async Task<Pret> SeedPretAsync(AkarangaBank.Web.Data.AkarangaBankContext db,
        Agence agence, Client client, Compte compte, int duree = 12)
    {
        var pret = new Pret
        {
            Reference = $"PRET-{Guid.NewGuid():N}"[..25], Client = client, CompteDebit = compte,
            TypePretId = 1, MontantPrincipal = 120000m, TauxInteret = 0m, DureeMois = duree,
            Mensualite = 120000m / duree, Statut = "EnAttente", DateDemande = DateTime.UtcNow
        };
        db.Prets.Add(pret);
        await db.SaveChangesAsync();
        return pret;
    }
}
