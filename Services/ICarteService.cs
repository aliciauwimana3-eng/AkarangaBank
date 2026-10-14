using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface ICarteService
{
    Task<TransactionCarte> AutoriserTransactionAsync(int carteId, int utilisateurId, decimal montant, string typeTransaction);
    Task<IReadOnlyList<TransactionCarte>> GetHistoriqueAsync(int carteId, int utilisateurId);
    Task RenouvelerAsync(int carteId, int utilisateurId);
    Task BloquerAsync(int carteId, int utilisateurId, string motif);
}
