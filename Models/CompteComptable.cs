namespace AkarangaBank.Web.Models;

/// <summary>Compte de contrepartie du grand livre (caisse, banque, etc.).</summary>
public class CompteComptable
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Devise { get; set; } = "BIF";
    public bool EstActif { get; set; } = true;
    public ICollection<Ecriture> Ecritures { get; set; } = new List<Ecriture>();
}
