using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class SeuilsAMLViewModel
{
    [Display(Name = "Agence")]
    public int? AgenceId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true)]
    [Display(Name = "Seuil de vigilance AML (BIF)")]
    public decimal SeuilAML { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true)]
    [Display(Name = "Seuil de déclaration CNRF en espèces (BIF)")]
    public decimal SeuilDeclarationEspece { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true)]
    [Display(Name = "Seuil de blocage (BIF)")]
    public decimal SeuilBlocage { get; set; }
}
