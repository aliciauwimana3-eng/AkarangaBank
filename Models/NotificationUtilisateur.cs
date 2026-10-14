namespace AkarangaBank.Web.Models;

public class NotificationUtilisateur
{
    public int Id { get; set; }
    public int UtilisateurId { get; set; }
    public ApplicationUser Utilisateur { get; set; } = null!;
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Url { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateLecture { get; set; }
    public string Canal { get; set; } = "Interne";
    public string Statut { get; set; } = "Envoyee";
    public string? TemplateCode { get; set; }
    public string Langue { get; set; } = "fr-BI";
    public int? TemplateVersion { get; set; }
    public int Tentatives { get; set; }
    public string? Erreur { get; set; }
    public DateTime? DateEnvoi { get; set; }
}
