using System.Globalization;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public class ConformiteService : IConformiteService
{
    private const decimal SeuilParDefaut = 5_000_000m;
    private const decimal SeuilDeclarationParDefaut = 10_000_000m;
    private const decimal SeuilBlocageParDefaut = 50_000_000m;
    private readonly AkarangaBankContext _context;
    public ConformiteService(AkarangaBankContext context) => _context = context;

    public Task<decimal> GetSeuilAMLAsync(int? agenceId = null) =>
        GetSeuilAsync("SEUIL_AML", agenceId, SeuilParDefaut);

    public Task<decimal> GetSeuilDeclarationEspeceAsync(int? agenceId = null) =>
        GetSeuilAsync("SEUIL_DECLARATION_ESPECE", agenceId, SeuilDeclarationParDefaut);

    public Task<decimal> GetSeuilBlocageAsync(int? agenceId = null) =>
        GetSeuilAsync("SEUIL_BLOCAGE", agenceId, SeuilBlocageParDefaut);

    private async Task<decimal> GetSeuilAsync(string cle, int? agenceId, decimal valeurParDefaut)
    {
        var valeur = agenceId.HasValue
            ? await _context.Parametres.AsNoTracking().Where(p => p.Cle == cle && p.AgenceId == agenceId)
                .Select(p => p.Valeur).FirstOrDefaultAsync()
            : null;
        valeur ??= await _context.Parametres.AsNoTracking()
            .Where(p => p.Cle == cle && p.AgenceId == null).Select(p => p.Valeur).FirstOrDefaultAsync();
        return decimal.TryParse(valeur, NumberStyles.Number, CultureInfo.InvariantCulture, out var seuil) && seuil > 0
            ? seuil : valeurParDefaut;
    }

    public async Task EnregistrerSeuilsAsync(decimal seuilAML, decimal seuilDeclarationEspece, int? agenceId = null, decimal? seuilBlocage = null)
    {
        if (seuilAML <= 0 || seuilDeclarationEspece <= 0 || seuilBlocage is <= 0)
            throw new InvalidOperationException("Les seuils renseignés doivent être strictement positifs.");
        if (seuilDeclarationEspece < seuilAML || seuilBlocage.HasValue && seuilBlocage.Value < seuilDeclarationEspece)
            throw new InvalidOperationException("Les seuils doivent respecter l'ordre vigilance ≤ déclaration ≤ blocage.");
        if (agenceId.HasValue && !await _context.Agences.AnyAsync(a => a.Id == agenceId.Value))
            throw new InvalidOperationException("Agence introuvable.");
        await EnregistrerSeuilAsync("SEUIL_AML", seuilAML, "Seuil de vigilance AML en BIF.", agenceId);
        await EnregistrerSeuilAsync("SEUIL_DECLARATION_ESPECE", seuilDeclarationEspece,
            "Seuil de déclaration CNRF des opérations en espèces, en BIF.", agenceId);
        if (seuilBlocage.HasValue)
            await EnregistrerSeuilAsync("SEUIL_BLOCAGE", seuilBlocage.Value, "Seuil de blocage AML en BIF.", agenceId);
        await _context.SaveChangesAsync();
    }

    private async Task EnregistrerSeuilAsync(string cle, decimal valeur, string description, int? agenceId)
    {
        var parametre = await _context.Parametres.FirstOrDefaultAsync(p => p.Cle == cle && p.AgenceId == agenceId);
        if (parametre is null)
        {
            _context.Parametres.Add(new Parametre
            {
                Cle = cle,
                Valeur = valeur.ToString(CultureInfo.InvariantCulture),
                Description = description,
                AgenceId = agenceId,
                DateModification = DateTime.UtcNow
            });
            return;
        }
        parametre.Valeur = valeur.ToString(CultureInfo.InvariantCulture);
        parametre.Description = description;
        parametre.DateModification = DateTime.UtcNow;
    }

    public async Task<ResultatConformite> AnalyserTransactionAsync(int compteId, decimal montant, string typeOperation)
    {
        var compte = await _context.Comptes.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == compteId)
            ?? throw new InvalidOperationException("Compte introuvable.");
        var resultat = new ResultatConformite { CompteId = compteId, Montant = montant, DateAnalyse = DateTime.UtcNow };

        if (montant <= 0)
        {
            resultat.EstBloquee = true;
            resultat.Motif = "Le montant doit être positif.";
            resultat.Niveau = NiveauAlerte.Critique;
        }
        var seuilAML = await GetSeuilAMLAsync(compte.AgenceId);
        var seuilDeclaration = await GetSeuilDeclarationEspeceAsync(compte.AgenceId);
        if ((typeOperation is "Depot" or "Retrait") && montant >= seuilDeclaration)
        {
            resultat.NecessiteDeclaration = true;
            resultat.Motif = $"Transaction en espèces ≥ {seuilDeclaration:N0} BIF. Déclaration obligatoire à la CNRF.";
            resultat.Niveau = NiveauAlerte.Eleve;
        }
        var seuilBlocage = await GetSeuilBlocageAsync(compte.AgenceId);
        if (montant >= seuilBlocage)
        {
            resultat.EstBloquee = true;
            resultat.Motif = $"Montant ≥ {seuilBlocage:N0} BIF : opération bloquée par le seuil AML.";
            resultat.Niveau = NiveauAlerte.Critique;
        }
        if (montant >= seuilAML)
        {
            resultat.NecessiteVigilanceRenforcee = true;
            resultat.Motif ??= $"Montant ≥ {seuilAML:N0} BIF (seuil AML). Vérification de l'origine des fonds requise.";
            if (resultat.Niveau is not (NiveauAlerte.Eleve or NiveauAlerte.Critique))
                resultat.Niveau = NiveauAlerte.Moyen;
        }

        if ((typeOperation is "Retrait" or "Virement" or "VirementInterbancaire") && compte.Solde < montant)
        {
            resultat.EstBloquee = true;
            resultat.Motif = "Solde insuffisant.";
            resultat.Niveau = NiveauAlerte.Critique;
        }
        if (compte.Statut != "Actif")
        {
            resultat.EstBloquee = true;
            resultat.Motif = $"Compte {compte.Statut}. Opération impossible.";
            resultat.Niveau = NiveauAlerte.Critique;
        }
        if (compte.Client.Statut != "Actif")
        {
            resultat.EstBloquee = true;
            resultat.Motif = "Le client n'est pas actif.";
            resultat.Niveau = NiveauAlerte.Critique;
        }
        return resultat;
    }

    public async Task<bool> DetecterFractionnementAsync(int compteId, decimal montant, DateTime date)
    {
        var agenceId = await _context.Comptes.AsNoTracking().Where(c => c.Id == compteId)
            .Select(c => (int?)c.AgenceId).FirstOrDefaultAsync();
        var seuil = await GetSeuilAMLAsync(agenceId);
        if (montant >= seuil) return false;
        var debut = date.AddHours(-24);
        var totalRecent = await _context.Ecritures.AsNoTracking()
            .Where(e => e.CompteId == compteId && e.DateEcriture >= debut && e.DateEcriture <= date && e.Montant < seuil)
            .SumAsync(e => (decimal?)e.Montant) ?? 0m;
        return totalRecent + montant >= seuil;
    }

    public async Task<NiveauRisqueClient> EvaluerRisqueClientAsync(int clientId)
    {
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);
        if (client is null) return NiveauRisqueClient.Inconnu;
        var score = 0;
        if (await _context.Comptes.CountAsync(c => c.ClientId == clientId) > 3) score += 2;
        var seuil = await GetSeuilAMLAsync(client.AgenceId);
        if (await _context.Ecritures.AnyAsync(e => e.CompteId != null && e.Compte!.ClientId == clientId && e.Montant >= seuil)) score += 3;
        if (await _context.AlertesAML.AnyAsync(a => a.ClientId == clientId)) score += 6;
        return score >= 6 ? NiveauRisqueClient.Eleve : score >= 3 ? NiveauRisqueClient.Moyen : NiveauRisqueClient.Faible;
    }
}

