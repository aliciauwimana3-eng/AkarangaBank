using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AkarangaBank.Web.ViewModels;

public class VerificationKYCCreateViewModel
{
    [Required]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Le type de document est obligatoire.")]
    [StringLength(50)]
    [Display(Name = "Type de document")]
    public string TypeDocument { get; set; } = "CNI";

    [Required(ErrorMessage = "Le numéro de document est obligatoire.")]
    [StringLength(50)]
    [Display(Name = "Numéro de document")]
    public string NumeroDocument { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le document justificatif est obligatoire.")]
    [Display(Name = "Document justificatif")]
    public IFormFile? Fichier { get; set; }

    [BindNever]
    public string? DocumentKey { get; set; }
    [BindNever]
    public string? NomDocumentOriginal { get; set; }
    [BindNever]
    public string? TypeMimeDocument { get; set; }
    [BindNever]
    public long? TailleDocument { get; set; }

    [StringLength(500)]
    public string? Commentaire { get; set; }

    // Pour l'affichage
    public string? ClientNom { get; set; }
    public string? ClientNumero { get; set; }
}
