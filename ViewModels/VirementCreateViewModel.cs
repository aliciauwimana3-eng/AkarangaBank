using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class VirementCreateViewModel : IValidatableObject
{
    [Display(Name = "Virement interbancaire")]
    public bool EstInterbancaire { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez un compte source.")]
    [Display(Name = "Compte source")]
    public int CompteSourceId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sélectionnez un compte destinataire.")]
    [Display(Name = "Compte destinataire")]
    public int? CompteDestinationId { get; set; }

    [StringLength(100)]
    [Display(Name = "Banque destinataire")]
    public string? BanqueDestination { get; set; }

    [StringLength(34)]
    [Display(Name = "Numéro de compte / IBAN")]
    public string? NumeroCompteExterne { get; set; }

    [StringLength(150)]
    [Display(Name = "Nom du bénéficiaire")]
    public string? BeneficiaireExterne { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Le montant doit être supérieur à zéro.")]
    [Display(Name = "Montant (BIF)")]
    public decimal Montant { get; set; }

    [StringLength(255)]
    [Display(Name = "Motif")]
    public string? Motif { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EstInterbancaire)
        {
            if (string.IsNullOrWhiteSpace(BanqueDestination))
                yield return new ValidationResult("Indiquez la banque destinataire.", [nameof(BanqueDestination)]);
            if (string.IsNullOrWhiteSpace(NumeroCompteExterne))
                yield return new ValidationResult("Indiquez le compte externe ou l'IBAN.", [nameof(NumeroCompteExterne)]);
            if (string.IsNullOrWhiteSpace(BeneficiaireExterne))
                yield return new ValidationResult("Indiquez le nom du bénéficiaire.", [nameof(BeneficiaireExterne)]);
        }
        else if (!CompteDestinationId.HasValue || CompteDestinationId.Value < 1)
        {
            yield return new ValidationResult("Sélectionnez un compte destinataire.", [nameof(CompteDestinationId)]);
        }
    }
}
