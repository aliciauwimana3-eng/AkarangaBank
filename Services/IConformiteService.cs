using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface IConformiteService
{
    Task<ResultatConformite> AnalyserTransactionAsync(int compteId, decimal montant, string typeOperation);
    Task<bool> DetecterFractionnementAsync(int compteId, decimal montant, DateTime date);
    Task<NiveauRisqueClient> EvaluerRisqueClientAsync(int clientId);
    Task<decimal> GetSeuilAMLAsync(int? agenceId = null);
    Task<decimal> GetSeuilDeclarationEspeceAsync(int? agenceId = null);
    Task<decimal> GetSeuilBlocageAsync(int? agenceId = null);
    Task EnregistrerSeuilsAsync(decimal seuilAML, decimal seuilDeclarationEspece, int? agenceId = null, decimal? seuilBlocage = null);
}
