using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class UtilisateurResetPasswordViewModel
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Au moins 8 caractères.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe")]
    public string NewPassword { get; set; } = string.Empty;
}
