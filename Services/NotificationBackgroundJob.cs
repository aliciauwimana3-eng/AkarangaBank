using System.Text.Json;
using AkarangaBank.Web.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public sealed class NotificationBackgroundJob
{
    private readonly AkarangaBankContext _db;
    private readonly INotificationTemplateService _templates;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;
    private readonly IPushNotificationService _push;
    private readonly INotificationService _notifications;
    public NotificationBackgroundJob(AkarangaBankContext db, INotificationTemplateService templates, IEmailService email, ISmsService sms, IPushNotificationService push, INotificationService notifications)
    { _db = db; _templates = templates; _email = email; _sms = sms; _push = push; _notifications = notifications; }

    public async Task PlanifierRappelsEcheancesAsync()
    {
        var date = DateTime.UtcNow.Date.AddDays(3);
        var users = await _db.EcheancesPret.AsNoTracking()
            .Where(e => e.Statut == "EnAttente" && (e.Pret.Statut == "Valide" || e.Pret.Statut == "Impayé") && e.DateEcheance >= date && e.DateEcheance < date.AddDays(1))
            .Select(e => new { e.Pret.ClientId, e.MontantTotal, e.DateEcheance, e.Pret.Client.Nom, e.Pret.Client.Prenom })
            .ToListAsync();
        foreach (var item in users)
        {
            var recipients = await _db.Users.AsNoTracking().Where(u => u.ClientId == item.ClientId && u.EstActif).Select(u => u.Id).ToListAsync();
            foreach (var recipient in recipients)
                await _notifications.CreerEtEnvoyerAsync(recipient, "SMS", "RAPPEL_ECHEANCE", "fr-BI", new Dictionary<string, string>
                {
                    ["NomClient"] = $"{item.Prenom} {item.Nom}", ["Montant"] = item.MontantTotal.ToString("N0"), ["DateEcheance"] = item.DateEcheance.ToString("dd/MM/yyyy")
                });
        }
    }

    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task DeliverAsync(int notificationId)
    {
        var item = await _db.Notifications.Include(n => n.Utilisateur).FirstOrDefaultAsync(n => n.Id == notificationId);
        if (item is null || item.Statut == "Envoyee") return;
        item.Tentatives++;
        try
        {
            var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(item.Message) ?? [];
            var rendered = await _templates.RenderAsync(item.TemplateCode!, item.Canal, item.Langue, variables);
            if (rendered is null) throw new InvalidOperationException("Template introuvable pour le canal et la langue demandes.");
            switch (item.Canal)
            {
                case "Email":
                    if (string.IsNullOrWhiteSpace(item.Utilisateur.Email)) throw new InvalidOperationException("Adresse email manquante.");
                    await _email.SendAsync(item.Utilisateur.Email, rendered.Subject, rendered.Text, rendered.Html);
                    break;
                case "SMS":
                    if (string.IsNullOrWhiteSpace(item.Utilisateur.PhoneNumber)) throw new InvalidOperationException("Numero de telephone manquant.");
                    await _sms.SendAsync(item.Utilisateur.PhoneNumber, rendered.Text);
                    break;
                case "Push":
                    if (string.IsNullOrWhiteSpace(item.Utilisateur.PushToken)) throw new InvalidOperationException("Jeton push manquant.");
                    await _push.SendAsync(item.Utilisateur.PushToken, rendered.Subject, rendered.Text);
                    break;
                default: throw new InvalidOperationException("Canal de notification inconnu.");
            }
            item.Titre = rendered.Subject;
            item.Message = rendered.Text;
            item.TemplateVersion = rendered.Version;
            item.Statut = "Envoyee";
            item.DateEnvoi = DateTime.UtcNow;
            item.Erreur = null;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            item.Statut = item.Tentatives >= 3 ? "Echec" : "EnAttente";
            item.Erreur = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
            await _db.SaveChangesAsync();
            if (item.Tentatives < 3) throw;
        }
    }
}
