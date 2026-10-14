using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Hangfire;

namespace AkarangaBank.Web.Services;

public sealed class NotificationService : INotificationService
{
    private static readonly HashSet<string> Canaux = ["Email", "SMS", "Push"];
    private readonly AkarangaBankContext _db;
    private readonly IBackgroundJobClient _jobs;
    public NotificationService(AkarangaBankContext db, IBackgroundJobClient jobs) { _db = db; _jobs = jobs; }

    public async Task<int> CreerEtEnvoyerAsync(int utilisateurId, string canal, string templateCode, string langue, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        if (!Canaux.Contains(canal)) throw new ArgumentException("Canal de notification non pris en charge.", nameof(canal));
        var record = new NotificationUtilisateur
        {
            UtilisateurId = utilisateurId,
            Canal = canal,
            Statut = "EnAttente",
            TemplateCode = templateCode,
            Langue = langue,
            Titre = templateCode,
            Message = System.Text.Json.JsonSerializer.Serialize(variables)
        };
        _db.Notifications.Add(record);
        await _db.SaveChangesAsync(cancellationToken);
        _jobs.Enqueue<NotificationBackgroundJob>(job => job.DeliverAsync(record.Id));
        return record.Id;
    }
}
