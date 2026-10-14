using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> TablesAuditees = new(StringComparer.OrdinalIgnoreCase)
    {
        "Clients", "Comptes", "Operations", "Virements", "Prets", "Ecritures",
        "AlertesAML", "Parametres", "ComptesComptables", "Utilisateurs"
    };

    private static readonly string[] TermesSensibles =
        ["password", "secret", "token", "cni", "telephone", "phone", "email", "compteexterne", "numerocompte"];

    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AjouterEntreesAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AjouterEntreesAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AjouterEntreesAudit(DbContext? context)
    {
        if (context is null) return;
        if (context.ChangeTracker.Entries<AuditLog>().Any(e => e.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Une entrée de piste d'audit ne peut pas être modifiée ni supprimée.");
        var http = _httpContextAccessor.HttpContext;
        var user = http?.User;
        int? userId = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        var entrees = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted &&
                        TablesAuditees.Contains(e.Metadata.GetTableName() ?? string.Empty))
            .ToList();

        foreach (var entry in entrees)
        {
            var action = entry.State.ToString();
            var avant = entry.State is EntityState.Modified or EntityState.Deleted ? LireValeurs(entry, avant: true) : null;
            var apres = entry.State is EntityState.Added or EntityState.Modified ? LireValeurs(entry, avant: false) : null;
            if (entry.State == EntityState.Modified && avant == apres) continue;

            context.Set<AuditLog>().Add(new AuditLog
            {
                DateEvenement = DateTime.UtcNow,
                UtilisateurId = userId,
                AgenceId = LireAgenceId(entry, context),
                Action = action,
                TableConcernee = entry.Metadata.GetTableName(),
                EnregistrementId = LireId(entry),
                ValeurAvant = avant,
                ValeurApres = apres,
                AdresseIP = Tronquer(http?.Connection.RemoteIpAddress?.ToString(), 45),
                UserAgent = Tronquer(http?.Request.Headers.UserAgent.ToString(), 255),
                Details = http is null ? "Modification hors requête HTTP." : $"{http.Request.Method} {http.Request.Path}"
            });
        }
    }

    private static string? LireValeurs(EntityEntry entry, bool avant)
    {
        var values = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            if (avant && entry.State == EntityState.Modified && !property.IsModified) continue;
            var value = avant ? property.OriginalValue : property.CurrentValue;
            values[property.Metadata.Name] = EstSensible(property.Metadata.Name) ? "[MASQUÉ]" : value;
        }
        return JsonSerializer.Serialize(values);
    }

    private static bool EstSensible(string name) =>
        TermesSensibles.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static int? LireId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
        if (key is null) return null;
        var property = entry.Property(key.Name);
        return property.IsTemporary ? null : Convert.ToInt32(property.CurrentValue);
    }

    private static int? LireAgenceId(EntityEntry entry, DbContext context)
    {
        var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "AgenceId");
        if (property?.CurrentValue is int value) return value;

        foreach (var navigationName in new[] { "Client", "Compte", "CompteSource", "CompteDestination", "Operation" })
        {
            if (entry.Metadata.FindNavigation(navigationName) is null) continue;
            var related = entry.Reference(navigationName).CurrentValue;
            if (related?.GetType().GetProperty("AgenceId")?.GetValue(related) is int relatedAgenceId)
                return relatedAgenceId;
        }

        foreach (var foreignKeyName in new[] { "CompteId", "CompteSourceId", "ClientId", "OperationId" })
        {
            var foreignKey = entry.Properties.FirstOrDefault(p => p.Metadata.Name == foreignKeyName)?.CurrentValue;
            if (foreignKey is not int relatedId) continue;
            var agenceId = foreignKeyName switch
            {
                "CompteId" or "CompteSourceId" => context.Set<Compte>().Local.FirstOrDefault(c => c.Id == relatedId)?.AgenceId,
                "ClientId" => context.Set<Client>().Local.FirstOrDefault(c => c.Id == relatedId)?.AgenceId,
                "OperationId" => context.Set<Operation>().Local.FirstOrDefault(o => o.Id == relatedId)?.AgenceId,
                _ => null
            };
            if (agenceId.HasValue) return agenceId;
        }
        return null;
    }

    private static string? Tronquer(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];
}
