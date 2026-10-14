using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class PretCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez un type de prêt.")]
    [Display(Name = "Type de prêt")]
    public int TypePretId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez un compte de versement.")]
    [Display(Name = "Compte de versement")]
    public int CompteDebitId { get; set; }

    [Range(typeof(decimal), "1", "9999999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Le montant doit être supérieur à zéro.")]
    [Display(Name = "Montant demandé")]
    public decimal MontantPrincipal { get; set; }

    [Range(1, 360, ErrorMessage = "La durée doit être comprise entre 1 et 360 mois.")]
    [Display(Name = "Durée (mois)")]
    public int DureeMois { get; set; }

    [StringLength(500)]
    [Display(Name = "Commentaire")]
    public string? Commentaire { get; set; }
}
