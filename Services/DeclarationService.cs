using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public class DeclarationService : IDeclarationService
{
    private static readonly string[] StatutsAutorisés = ["EnCours", "Traitee", "Classee"];
    private readonly AkarangaBankContext _context;
    private readonly INotificationService _notifications;
    private readonly ILogger<DeclarationService> _logger;
    public DeclarationService(AkarangaBankContext context, INotificationService notifications, ILogger<DeclarationService> logger)
    {
        _context = context;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<AlerteAML> CreerAlerteAsync(int clientId, int? operationId, string type, string niveau, string? description)
    {
        var exists = await _context.Clients.AnyAsync(c => c.Id == clientId);
        if (!exists) throw new InvalidOperationException("Client introuvable pour l'alerte AML.");
        var alerte = new AlerteAML
        {
            ClientId = clientId,
            OperationId = operationId,
            TypeAlerte = type.Length > 50 ? type[..50] : type,
            Niveau = Enum.TryParse<NiveauAlerte>(niveau, true, out var parsed) ? parsed.ToString() : NiveauAlerte.Moyen.ToString(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description[..Math.Min(description.Length, 500)],
            Statut = "Ouverte",
            DateAlerte = DateTime.UtcNow
        };
        _context.AlertesAML.Add(alerte);
        await _context.SaveChangesAsync();
        if (alerte.Niveau == NiveauAlerte.Critique.ToString())
        {
            var agencyId = await _context.Clients.AsNoTracking().Where(c => c.Id == clientId).Select(c => c.AgenceId).FirstAsync();
            var recipients = await _context.Users.AsNoTracking()
                .Where(u => u.EstActif && u.Email != null &&
                    (u.Role == "admin" || (u.AgenceId == agencyId && (u.Role == "gestionnaire" || u.Role == "auditeur"))))
                .Select(u => u.Id).ToListAsync();
            foreach (var recipient in recipients)
            {
                try
                {
                    await _notifications.CreerEtEnvoyerAsync(recipient, "Email", "ALERTE_AML_CRITIQUE", "fr-BI", new Dictionary<string, string>
                    {
                        ["Reference"] = $"AML-{alerte.Id}", ["NomClient"] = clientId.ToString(), ["Montant"] = string.Empty
                    });
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Impossible de planifier la notification de l'alerte AML critique {AlertId}.", alerte.Id);
                }
            }
        }
        return alerte;
    }

    public async Task<IReadOnlyList<AlerteAML>> GetAlertesAsync(string? statut, string? niveau, int utilisateurId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null || user.Role == "client" || user.Role == "guichetier") return [];
        var query = _context.AlertesAML.AsNoTracking().AsQueryable();
        if (user.Role != "admin") query = query.Where(a => a.Client.AgenceId == user.AgenceId);
        if (!string.IsNullOrWhiteSpace(statut)) query = query.Where(a => a.Statut == statut);
        if (!string.IsNullOrWhiteSpace(niveau)) query = query.Where(a => a.Niveau == niveau);
        return await query.Include(a => a.Client).Include(a => a.Operation)
            .OrderByDescending(a => a.DateAlerte).Take(300).ToListAsync();
    }

    public async Task<IReadOnlyList<AlerteAML>> GetDeclarationsCNRFAsync(int utilisateurId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null || user.Role is "client" or "guichetier") return [];
        var query = _context.AlertesAML.AsNoTracking().Where(a => a.TypeAlerte == "DeclarationCNRF");
        if (user.Role != "admin") query = query.Where(a => a.Client.AgenceId == user.AgenceId);
        return await query.Include(a => a.Client).Include(a => a.Operation)
            .OrderByDescending(a => a.DateAlerte).Take(2000).ToListAsync();
    }

    public async Task<bool> TraiterAlerteAsync(int alerteId, int traiteParId, string statut, string? commentaire)
    {
        if (!StatutsAutorisés.Contains(statut)) throw new InvalidOperationException("Statut de traitement invalide.");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == traiteParId && u.EstActif);
        if (user is null || user.Role is not ("admin" or "gestionnaire"))
            throw new InvalidOperationException("Seul un gestionnaire ou administrateur actif peut traiter une alerte.");
        var alerte = await _context.AlertesAML.Include(a => a.Client).FirstOrDefaultAsync(a => a.Id == alerteId);
        if (alerte is null) return false;
        if (user.Role != "admin" && alerte.Client.AgenceId != user.AgenceId)
            throw new InvalidOperationException("Cette alerte ne relève pas de votre agence.");
        if (alerte.Statut is "Traitee" or "Classee") throw new InvalidOperationException("Cette alerte est déjà clôturée.");
        alerte.Statut = statut;
        alerte.TraiteParId = user.Id;
        alerte.DateTraitement = statut is "Traitee" or "Classee" ? DateTime.UtcNow : null;
        if (!string.IsNullOrWhiteSpace(commentaire))
        {
            var note = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm} — {commentaire.Trim()}";
            alerte.Description = string.IsNullOrWhiteSpace(alerte.Description) ? note : $"{alerte.Description}\n{note}";
            if (alerte.Description.Length > 500) alerte.Description = alerte.Description[^500..];
        }
        await _context.SaveChangesAsync();
        return true;
    }
}
