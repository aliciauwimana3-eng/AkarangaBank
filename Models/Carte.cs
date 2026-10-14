namespace AkarangaBank.Web.Models;

public class Carte
{
    public int Id { get; set; }
    public string NumeroCarte { get; set; } = string.Empty;
    public int CompteId { get; set; }
    public Compte Compte { get; set; } = null!;
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public string TypeCarte { get; set; } = string.Empty;
    public string NumeroCarteChiffre { get; set; } = string.Empty;
    public string DerniersChiffres { get; set; } = string.Empty;
    public string NumeroMasque => $"**** **** **** {DerniersChiffres}";
    public decimal PlafondRetraitJournalier { get; set; } = 500000m;
    public decimal PlafondPaiementJournalier { get; set; } = 1000000m;
    public decimal PlafondEnLigneJournalier { get; set; } = 300000m;
    public DateTime DateExpiration { get; set; }
    public string Statut { get; set; } = "Active";
    public string? MotifBlocage { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public ICollection<TransactionCarte> Transactions { get; set; } = new List<TransactionCarte>();
}
