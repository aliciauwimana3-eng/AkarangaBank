namespace AkarangaBank.Web.Models;

public class DocumentAcces
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public int UtilisateurId { get; set; }
    public ApplicationUser Utilisateur { get; set; } = null!;
    public string Action { get; set; } = "Telechargement";
    public DateTime DateAcces { get; set; } = DateTime.UtcNow;
}
