using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class CarteCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Choisissez un compte.")]
    public int CompteId { get; set; }

    [Required, RegularExpression("^(Visa|MasterCard|Locale)$", ErrorMessage = "Choisissez Visa, MasterCard ou Locale.")]
    public string TypeCarte { get; set; } = "Visa";

    [Range(typeof(decimal), "1", "9999999999999999", ParseLimitsInInvariantCulture = true)] public decimal PlafondRetraitJournalier { get; set; } = 500000m;
    [Range(typeof(decimal), "1", "9999999999999999", ParseLimitsInInvariantCulture = true)] public decimal PlafondPaiementJournalier { get; set; } = 1000000m;
    [Range(typeof(decimal), "1", "9999999999999999", ParseLimitsInInvariantCulture = true)] public decimal PlafondEnLigneJournalier { get; set; } = 300000m;
}
