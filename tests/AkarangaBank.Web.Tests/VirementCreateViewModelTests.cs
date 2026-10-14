using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Tests;

public class VirementCreateViewModelTests
{
    [Fact]
    public void VirementInterne_AvecDeuxComptes_EstValide()
    {
        var demande = new VirementCreateViewModel
        {
            CompteSourceId = 1,
            CompteDestinationId = 2,
            Montant = 25000m
        };

        Assert.Empty(Valider(demande));
    }

    [Fact]
    public void VirementInterbancaire_AvecBeneficiaire_EstValideSansCompteInterneDestination()
    {
        var demande = new VirementCreateViewModel
        {
            CompteSourceId = 1,
            EstInterbancaire = true,
            BanqueDestination = "Banque B",
            NumeroCompteExterne = "BI00BANK00012345",
            BeneficiaireExterne = "Client B",
            Montant = 25000m
        };

        Assert.Empty(Valider(demande));
    }

    [Fact]
    public void VirementInterbancaire_SansCoordonneesBeneficiaire_EstInvalide()
    {
        var demande = new VirementCreateViewModel
        {
            CompteSourceId = 1,
            EstInterbancaire = true,
            Montant = 25000m
        };

        var erreurs = Valider(demande);

        Assert.Contains(erreurs, e => e.MemberNames.Contains(nameof(demande.BanqueDestination)));
        Assert.Contains(erreurs, e => e.MemberNames.Contains(nameof(demande.NumeroCompteExterne)));
        Assert.Contains(erreurs, e => e.MemberNames.Contains(nameof(demande.BeneficiaireExterne)));
    }

    [Fact]
    public void Montant_ValideSousCultureFrancaise()
    {
        var cultureAvant = CultureInfo.CurrentCulture;
        var cultureUiAvant = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            var demande = new VirementCreateViewModel
            {
                CompteSourceId = 1, CompteDestinationId = 2, Montant = 0.01m
            };

            Assert.Empty(Valider(demande));
        }
        finally
        {
            CultureInfo.CurrentCulture = cultureAvant;
            CultureInfo.CurrentUICulture = cultureUiAvant;
        }
    }

    private static List<ValidationResult> Valider(VirementCreateViewModel demande)
    {
        var erreurs = new List<ValidationResult>();
        Validator.TryValidateObject(demande, new ValidationContext(demande), erreurs, validateAllProperties: true);
        return erreurs;
    }
}
