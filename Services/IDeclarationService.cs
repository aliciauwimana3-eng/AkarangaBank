using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface IDeclarationService
{
    Task<AlerteAML> CreerAlerteAsync(int clientId, int? operationId, string type, string niveau, string? description);
    Task<bool> TraiterAlerteAsync(int alerteId, int traiteParId, string statut, string? commentaire);
    Task<IReadOnlyList<AlerteAML>> GetAlertesAsync(string? statut, string? niveau, int utilisateurId);
    Task<IReadOnlyList<AlerteAML>> GetDeclarationsCNRFAsync(int utilisateurId);
}
