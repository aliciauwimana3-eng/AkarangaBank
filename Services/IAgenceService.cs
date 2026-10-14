using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IAgenceService
{
    // Liste paginée avec recherche
    Task<AgenceListViewModel> GetPagedAsync(string? recherche, int page, int taille);

    // Détails avec compteurs
    Task<AgenceDetailsViewModel?> GetDetailsAsync(int id);

    // Récupération simple (pour Edit/Delete)
    Task<Agence?> GetByIdAsync(int id);

    Task<Agence> CreateAsync(AgenceCreateViewModel vm);
    Task<bool> UpdateAsync(AgenceEditViewModel vm);
    Task<bool> DeleteAsync(int id);

    // Vérifications métier
    Task<bool> CodeExisteAsync(string code, int? excludeId = null);
    Task<bool> APeuEtreSupprimeeAsync(int id);
}
