namespace AkarangaBank.Web.Models;

public class Document
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int? DeposeParId { get; set; }
    public ApplicationUser? DeposePar { get; set; }
    public string Categorie { get; set; } = "KYC";
    public string NomOriginal { get; set; } = string.Empty;
    public string CleStockage { get; set; } = string.Empty;
    public string TypeMime { get; set; } = "application/octet-stream";
    public long TailleOctets { get; set; }
    public string HashSha256 { get; set; } = string.Empty;
    public string Statut { get; set; } = "Actif";
    public int Version { get; set; } = 1;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateArchivage { get; set; }
    public DateTime? DateSuppression { get; set; }
    public ICollection<DocumentAcces> Acces { get; set; } = new List<DocumentAcces>();
}
