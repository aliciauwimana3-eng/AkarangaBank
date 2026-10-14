using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class OperationCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez un compte.")]
    [Display(Name = "Compte")]
    public int CompteId { get; set; }

    [Required(ErrorMessage = "Choisissez un type d'opération.")]
    [RegularExpression("^(Depot|Retrait)$", ErrorMessage = "Type d'opération invalide.")]
    [Display(Name = "Type d'opération")]
    public string TypeOperation { get; set; } = "Depot";

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Le montant doit être supérieur à zéro.")]
    [Display(Name = "Montant")]
    public decimal Montant { get; set; }

    [StringLength(255)]
    [Display(Name = "Commentaire")]
    public string? Commentaire { get; set; }
}
