using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IComptabiliteService
{
    Task<JournalViewModel> GetJournalAsync(DateTime? du, DateTime? au, string? typeOperation, int page, int taille, int utilisateurId);
    Task<ReleveViewModel?> GetReleveAsync(int compteId, int utilisateurId);
    Task<decimal> GetSoldeAsync(int compteId, int utilisateurId);
    Task<bool> VerifierEquilibreGlobalAsync();
}
