using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IPretService
{
    Task<IReadOnlyList<Compte>> GetComptesEligiblesAsync(int utilisateurId);
    Task<IReadOnlyList<Pret>> GetRecentAsync(int utilisateurId);
    Task<Pret?> GetDetailsAsync(int id, int utilisateurId);
    Task<Pret> CreateAsync(PretCreateViewModel vm, int utilisateurId);
    Task<bool> TraiterAsync(int id, int utilisateurId, bool approuver, decimal? tauxPersonnalise = null, string? motifRejet = null);
    Task<bool> PayerEcheanceAsync(int echeanceId, int utilisateurId);
    Task<int> MettreAJourRetardsAsync(DateTime? dateReference = null);
}
