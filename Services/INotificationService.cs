namespace AkarangaBank.Web.Services;

public interface INotificationService
{
    Task<int> CreerEtEnvoyerAsync(int utilisateurId, string canal, string templateCode, string langue, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken = default);
}
