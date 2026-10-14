using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IOperationService
{
    Task<IReadOnlyList<Compte>> GetComptesActifsAsync(int utilisateurId);
    Task<IReadOnlyList<Operation>> GetRecentAsync(int utilisateurId);
    Task<Operation> CreateAsync(OperationCreateViewModel vm, int utilisateurId);
    Task<bool> ValiderAsync(int id, int utilisateurId);
    Task<bool> RejeterAsync(int id, int utilisateurId);
}
