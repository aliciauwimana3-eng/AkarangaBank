namespace AkarangaBank.Web.Models;

public class TransactionCarte
{
    public int Id { get; set; }
    public int CarteId { get; set; }
    public Carte Carte { get; set; } = null!;
    public int? OperationId { get; set; }
    public Operation? Operation { get; set; }
    public decimal Montant { get; set; }
    public string TypeTransaction { get; set; } = "Paiement"; // Retrait, Paiement, EnLigne
    public string Statut { get; set; } = "Refusee"; // Autorisee, Refusee
    public string? MotifRefus { get; set; }
    public DateTime DateTransaction { get; set; } = DateTime.UtcNow;
}
