using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public class SurveillanceService : ISurveillanceService
{
    private readonly AkarangaBankContext _context;
    private readonly IConformiteService _conformite;
    public SurveillanceService(AkarangaBankContext context, IConformiteService conformite)
    {
        _context = context;
        _conformite = conformite;
    }

    public async Task<ResultatSurveillance> AnalyserTransactionAsync(int compteId, decimal montant, string typeOperation, DateTime date)
    {
        var resultat = new ResultatSurveillance { CompteId = compteId, DateAnalyse = date };
        var compte = await _context.Comptes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == compteId);
        if (compte is null) return resultat;
        var seuil = await _conformite.GetSeuilAMLAsync(compte.AgenceId);
        if (montant >= seuil)
        {
            resultat.Alertes.Add(new AlerteSurveillance
            {
                Regle = "MontantEleve",
                Description = $"Transaction de {montant:N0} {compte.Devise} égale ou supérieure au seuil AML.",
                Niveau = NiveauAlerte.Moyen
            });
        }
        var debut = date.AddDays(-7);
        var nombre = await _context.Ecritures.AsNoTracking()
            .Where(e => e.CompteId == compteId && e.Operation.Statut == "Valide" &&
                        e.Operation.DateOperation >= debut && e.Operation.DateOperation <= date)
            .Select(e => e.OperationId).Distinct().CountAsync();
        if (nombre > 20)
            resultat.Alertes.Add(new AlerteSurveillance
            {
                Regle = "FrequenceElevee",
                Description = $"{nombre} opérations enregistrées sur ce compte en sept jours.",
                Niveau = NiveauAlerte.Eleve
            });
        var risqueClient = await _conformite.EvaluerRisqueClientAsync(compte.ClientId);
        if (risqueClient == NiveauRisqueClient.Eleve)
            resultat.Alertes.Add(new AlerteSurveillance
            {
                Regle = "ClientARisque",
                Description = "Le client associé est classé à risque élevé.",
                Niveau = NiveauAlerte.Eleve
            });

        var moyenne = await _context.Ecritures.AsNoTracking()
            .Where(e => e.CompteId == compteId && e.Operation.Statut == "Valide")
            .GroupBy(e => e.OperationId)
            .Select(g => (decimal?)g.Max(e => e.Operation.Montant))
            .AverageAsync() ?? 0m;
        if (moyenne > 0 && montant > moyenne * 5)
            resultat.Alertes.Add(new AlerteSurveillance
            {
                Regle = "MontantInhabituel",
                Description = $"Montant {montant:N0} {compte.Devise} supérieur à cinq fois la moyenne ({moyenne:N0} {compte.Devise}).",
                Niveau = NiveauAlerte.Moyen
            });

        if (await _conformite.DetecterFractionnementAsync(compteId, montant, date))
        {
            resultat.Alertes.Add(new AlerteSurveillance
            {
                Regle = "Fractionnement",
                Description = "Plusieurs opérations sous le seuil AML dépassent ensemble ce seuil sur 24 heures.",
                Niveau = NiveauAlerte.Eleve
            });
        }
        if (typeOperation == "Depot")
        {
            var debutSmurfing = date.AddHours(-24);
            var depotsRecents = await _context.Ecritures.AsNoTracking()
                .Where(e => e.CompteId != null && e.Compte!.ClientId == compte.ClientId &&
                    e.Operation.TypeOperation == "Depot" && e.Operation.Statut == "Valide" &&
                    e.DateEcriture >= debutSmurfing && e.DateEcriture <= date && e.Montant < seuil)
                .GroupBy(e => e.CompteId)
                .Select(g => new { CompteId = g.Key, Montant = g.Sum(e => e.Montant) })
                .ToListAsync();
            var comptesConcernés = depotsRecents.Select(x => x.CompteId).Append((int?)compteId).Distinct().Count();
            if (comptesConcernés >= 3 && depotsRecents.Sum(x => x.Montant) + montant >= seuil)
                resultat.Alertes.Add(new AlerteSurveillance
                {
                    Regle = "Smurfing",
                    Description = "Des dépôts fractionnés sur plusieurs comptes du même client atteignent le seuil AML sur 24 heures.",
                    Niveau = NiveauAlerte.Eleve
                });
        }
        if (typeOperation.StartsWith("Virement", StringComparison.OrdinalIgnoreCase))
        {
            var debutLayering = date.AddHours(-24);
            var transferts = _context.Virements.AsNoTracking().Where(v =>
                v.DateVirement >= debutLayering && v.DateVirement <= date &&
                (v.Statut == "Valide" || v.Statut == "PretATransmettre") &&
                (v.CompteSource.ClientId == compte.ClientId || v.CompteDestination != null && v.CompteDestination.ClientId == compte.ClientId));
            var nombreTransferts = await transferts.CountAsync();
            var montantTransfere = await transferts.SumAsync(v => (decimal?)v.Montant) ?? 0m;
            if (nombreTransferts >= 3 && montantTransfere >= seuil)
                resultat.Alertes.Add(new AlerteSurveillance
                {
                    Regle = "Layering",
                    Description = "Plusieurs virements rapides liés au client dépassent le seuil AML cumulé sur 24 heures.",
                    Niveau = NiveauAlerte.Eleve
                });
        }
        resultat.NecessiteDeclaration = resultat.Alertes.Any(a => (int)a.Niveau >= (int)NiveauAlerte.Eleve);
        return resultat;
    }

    public Task<NiveauRisqueClient> EvaluerRisqueClientAsync(int clientId) => _conformite.EvaluerRisqueClientAsync(clientId);
}
