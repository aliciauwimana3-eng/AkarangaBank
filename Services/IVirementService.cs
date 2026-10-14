using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IVirementService
{
    Task<IReadOnlyList<Compte>> GetComptesActifsAsync(int utilisateurId);
    Task<IReadOnlyList<Virement>> GetRecentAsync(int utilisateurId);
    Task<Virement> CreateAsync(VirementCreateViewModel vm, int utilisateurId);
    Task<bool> ValiderAsync(int id, int utilisateurId);
    Task<bool> RejeterAsync(int id, int utilisateurId);
}
