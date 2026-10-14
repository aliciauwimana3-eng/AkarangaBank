using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Tests;

public class ProduitsBancairesViewModelTests
{
    [Theory]
    [InlineData("Visa", true)]
    [InlineData("MasterCard", true)]
    [InlineData("Locale", true)]
    [InlineData("Debit", false)]
    [InlineData("", false)]
    public void TypeCarte_EstLimiteAuxTypesSupportes(string type, bool valide)
    {
        var model = new CarteCreateViewModel { CompteId = 1, TypeCarte = type };
        Assert.Equal(valide, Valider(model).Count == 0);
    }

    [Fact]
    public void DemandePret_ValideLesDecimauxSousCultureFrancaise()
    {
        var cultureAvant = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var model = new PretCreateViewModel
            {
                TypePretId = 1, CompteDebitId = 1, MontantPrincipal = 1000m, DureeMois = 12
            };
            Assert.Empty(Valider(model));
        }
        finally { CultureInfo.CurrentCulture = cultureAvant; }
    }

    private static List<ValidationResult> Valider(object model)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties: true);
        return errors;
    }
}
