using System.Data;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public class VirementService : IVirementService
{
    private readonly AkarangaBankContext _context;
    private readonly IConformiteService _conformite;
    private readonly ISurveillanceService _surveillance;
    private readonly IDeclarationService _declaration;

    public VirementService(AkarangaBankContext context, IConformiteService conformite,
        ISurveillanceService surveillance, IDeclarationService declaration)
    {
        _context = context;
        _conformite = conformite;
        _surveillance = surveillance;
        _declaration = declaration;
    }

    public async Task<IReadOnlyList<Compte>> GetComptesActifsAsync(int utilisateurId)
    {
        var utilisateur = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId);
        if (utilisateur is null) return [];
        var comptes = _context.Comptes.AsNoTracking().AsQueryable();
        if (utilisateur.Role == "client")
        {
            if (!utilisateur.ClientId.HasValue) return [];
            comptes = comptes.Where(c => c.ClientId == utilisateur.ClientId.Value);
        }
        else if (utilisateur.Role != "admin")
            comptes = comptes.Where(c => c.AgenceId == utilisateur.AgenceId);
        return await comptes
            .Where(c => c.Statut == "Actif")
            .Include(c => c.Client)
            .OrderBy(c => c.NumeroCompte)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Virement>> GetRecentAsync(int utilisateurId)
    {
        var utilisateur = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId);
        if (utilisateur is null) return [];
        var virements = _context.Virements.AsNoTracking().AsQueryable();
        if (utilisateur.Role == "client")
        {
            if (!utilisateur.ClientId.HasValue) return [];
            virements = virements.Where(v => v.CompteSource.ClientId == utilisateur.ClientId.Value ||
                (v.CompteDestination != null && v.CompteDestination.ClientId == utilisateur.ClientId.Value));
        }
        else if (utilisateur.Role != "admin")
            virements = virements.Where(v => v.CompteSource.AgenceId == utilisateur.AgenceId ||
                (v.CompteDestination != null && v.CompteDestination.AgenceId == utilisateur.AgenceId));
        return await virements
            .Include(v => v.CompteSource).ThenInclude(c => c.Client)
            .Include(v => v.CompteDestination).ThenInclude(c => c!.Client)
            .Include(v => v.Utilisateur)
            .OrderByDescending(v => v.DateVirement)
            .Take(100)
            .ToListAsync();
    }

    public async Task<Virement> CreateAsync(VirementCreateViewModel vm, int utilisateurId)
    {
        if (vm.Montant <= 0) throw new InvalidOperationException("Le montant du virement doit être supérieur à zéro.");
        if (vm.EstInterbancaire && (string.IsNullOrWhiteSpace(vm.BanqueDestination) ||
            string.IsNullOrWhiteSpace(vm.NumeroCompteExterne) || string.IsNullOrWhiteSpace(vm.BeneficiaireExterne)))
            throw new InvalidOperationException("La banque, le compte externe et le bénéficiaire sont obligatoires pour un virement interbancaire.");
        var utilisateur = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (utilisateur is null)
            throw new InvalidOperationException("L'utilisateur connecté est introuvable ou désactivé.");

        if (!vm.EstInterbancaire && vm.CompteSourceId == vm.CompteDestinationId)
            throw new InvalidOperationException("Les comptes source et destinataire doivent être différents.");

        var source = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == vm.CompteSourceId);
        var destination = vm.EstInterbancaire || !vm.CompteDestinationId.HasValue
            ? null
            : await _context.Comptes.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == vm.CompteDestinationId.Value);
        if (source is null || (!vm.EstInterbancaire && destination is null))
            throw new InvalidOperationException("Le compte source ou destinataire est introuvable.");
        if (utilisateur.Role == "client" && source.ClientId != utilisateur.ClientId)
            throw new InvalidOperationException("Vous ne pouvez effectuer un virement qu'à partir de votre propre compte.");
        if (utilisateur.Role != "client" && utilisateur.Role != "admin" && source.AgenceId != utilisateur.AgenceId)
            throw new InvalidOperationException("Le compte source n'appartient pas à votre agence.");
        if (source.Statut != "Actif" || (destination is not null && destination.Statut != "Actif"))
            throw new InvalidOperationException("Les deux comptes doivent être actifs.");
        if (destination is not null && source.Devise != destination.Devise)
            throw new InvalidOperationException("Les comptes doivent utiliser la même devise.");
        if (destination is not null && destination.Client.Statut != "Actif")
            throw new InvalidOperationException("Le client du compte destinataire n'est pas actif.");

        var virement = new Virement
        {
            Reference = $"VIR-{Guid.NewGuid():N}"[..26].ToUpperInvariant(),
            CompteSourceId = source.Id,
            CompteSource = source,
            CompteDestinationId = destination?.Id,
            CompteDestination = destination,
            EstInterbancaire = vm.EstInterbancaire,
            BanqueDestination = vm.EstInterbancaire ? vm.BanqueDestination?.Trim() : null,
            NumeroCompteExterne = vm.EstInterbancaire ? vm.NumeroCompteExterne?.Trim() : null,
            BeneficiaireExterne = vm.EstInterbancaire ? vm.BeneficiaireExterne?.Trim() : null,
            UtilisateurId = utilisateurId,
            Montant = vm.Montant,
            Devise = source.Devise,
            Statut = "EnAttente",
            Motif = string.IsNullOrWhiteSpace(vm.Motif) ? null : vm.Motif.Trim(),
            DateVirement = DateTime.UtcNow
        };

        _context.Virements.Add(virement);
        await _context.SaveChangesAsync();
        return virement;
    }

    public async Task<bool> ValiderAsync(int id, int utilisateurId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var virement = await _context.Virements.FirstOrDefaultAsync(v => v.Id == id);
        if (virement is null) return false;
        if (virement.Statut != "EnAttente")
            throw new InvalidOperationException("Ce virement a déjà été traité.");
        if (virement.Montant <= 0 || (virement.CompteDestinationId.HasValue &&
            virement.CompteDestinationId.Value == virement.CompteSourceId))
            throw new InvalidOperationException("Ce virement contient un montant ou des comptes invalides.");

        var source = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == virement.CompteSourceId);
        var destination = virement.CompteDestinationId.HasValue
            ? await _context.Comptes.Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == virement.CompteDestinationId.Value)
            : null;
        var utilisateur = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (source is null || utilisateur is null || (!virement.EstInterbancaire && destination is null))
            throw new InvalidOperationException("Un compte ou l'utilisateur validateur est introuvable.");
        if (utilisateur.Role != "admin" && source.AgenceId != utilisateur.AgenceId)
            throw new InvalidOperationException("Ce virement ne relève pas de votre agence.");
        if (source.Statut != "Actif" || (destination is not null && destination.Statut != "Actif"))
            throw new InvalidOperationException("Les deux comptes doivent être actifs pour valider le virement.");
        if (destination is not null && destination.Client.Statut != "Actif")
            throw new InvalidOperationException("Le client destinataire n'est plus actif.");
        if ((destination is not null && source.Devise != destination.Devise) || source.Devise != virement.Devise)
            throw new InvalidOperationException("La devise des comptes ne correspond plus à celle du virement.");
        if (source.Solde < virement.Montant)
            throw new InvalidOperationException("Le solde du compte source est insuffisant.");

        var analyse = await _conformite.AnalyserTransactionAsync(source.Id, virement.Montant,
            virement.EstInterbancaire ? "VirementInterbancaire" : "Virement");
        if (analyse.EstBloquee) throw new InvalidOperationException(analyse.Motif ?? "Virement bloqué par les contrôles.");
        var dateAnalyse = DateTime.UtcNow;
        var fractionnement = await _conformite.DetecterFractionnementAsync(source.Id, virement.Montant, dateAnalyse);
        var surveillance = await _surveillance.AnalyserTransactionAsync(source.Id, virement.Montant,
            virement.EstInterbancaire ? "VirementInterbancaire" : "Virement", dateAnalyse);

        var now = DateTime.UtcNow;
        var operation = new Operation
        {
            Reference = $"{virement.Reference}-OP",
            TypeOperation = virement.EstInterbancaire ? "VirementInterbancaire" : "Virement",
            CompteId = source.Id,
            UtilisateurId = utilisateurId,
            AgenceId = source.AgenceId,
            VirementId = virement.Id,
            Montant = virement.Montant,
            Devise = virement.Devise,
            Statut = "Valide",
            DateOperation = now,
            DateValidation = now,
            Commentaire = virement.Motif
        };
        _context.Operations.Add(operation);
        await _context.SaveChangesAsync();

        if (virement.EstInterbancaire)
        {
            var codeReglement = $"VIREMENT-EXTERNE-{virement.Devise}";
            var compteReglement = await _context.ComptesComptables
                .FirstOrDefaultAsync(c => c.Code == codeReglement && c.EstActif);
            if (compteReglement is null)
            {
                compteReglement = new CompteComptable
                {
                    Code = codeReglement,
                    Libelle = $"Règlement des virements externes {virement.Devise}",
                    Devise = virement.Devise
                };
                _context.ComptesComptables.Add(compteReglement);
            }
            _context.Ecritures.AddRange(
                new Ecriture { OperationId = operation.Id, AgenceId = source.AgenceId, CompteId = source.Id, Sens = "Debit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Virement interbancaire {virement.Reference}" },
                new Ecriture { OperationId = operation.Id, AgenceId = source.AgenceId, CompteComptable = compteReglement, Sens = "Credit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Règlement externe {virement.BanqueDestination} — {virement.NumeroCompteExterne}" });
        }
        else if (source.AgenceId != destination!.AgenceId)
        {
            // Chaque agence porte sa propre contrepartie de compensation; les quatre lignes
            // rendent visibles les deux côtés du règlement inter-agences dans les balances.
            var compensationSource = await GetOrCreateLedgerAccountAsync(
                $"COMP-OUT-{source.AgenceId}-{virement.Devise}", $"Compensation sortante agence {source.AgenceId} {virement.Devise}", virement.Devise);
            var compensationDestination = await GetOrCreateLedgerAccountAsync(
                $"COMP-IN-{destination.AgenceId}-{virement.Devise}", $"Compensation entrante agence {destination.AgenceId} {virement.Devise}", virement.Devise);
            _context.Ecritures.AddRange(
                new Ecriture { OperationId = operation.Id, AgenceId = source.AgenceId, CompteId = source.Id, Sens = "Debit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Virement sortant {virement.Reference}" },
                new Ecriture { OperationId = operation.Id, AgenceId = source.AgenceId, CompteComptable = compensationSource, Sens = "Credit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Compensation inter-agences {virement.Reference}" },
                new Ecriture { OperationId = operation.Id, AgenceId = destination.AgenceId, CompteComptable = compensationDestination, Sens = "Debit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Compensation inter-agences {virement.Reference}" },
                new Ecriture { OperationId = operation.Id, AgenceId = destination.AgenceId, CompteId = destination.Id, Sens = "Credit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Virement entrant {virement.Reference}" });
        }
        else
        {
            _context.Ecritures.AddRange(
                new Ecriture { OperationId = operation.Id, AgenceId = source.AgenceId, CompteId = source.Id, Sens = "Debit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Virement sortant {virement.Reference}" },
                new Ecriture { OperationId = operation.Id, AgenceId = destination!.AgenceId, CompteId = destination.Id, Sens = "Credit", Montant = virement.Montant, Devise = virement.Devise, DateEcriture = now, Libelle = $"Virement entrant {virement.Reference}" });
        }
        source.Solde -= virement.Montant;
        if (destination is not null)
        {
            destination.Solde += virement.Montant;
            destination.DateModification = now;
        }
        source.DateModification = now;
        virement.Statut = virement.EstInterbancaire ? "PretATransmettre" : "Valide";
        virement.ControleAML = analyse.NecessiteVigilanceRenforcee || analyse.NecessiteDeclaration || fractionnement || surveillance.NecessiteDeclaration;
        virement.DateValidation = now;
        await _context.SaveChangesAsync();
        if (analyse.NecessiteVigilanceRenforcee)
            await _declaration.CreerAlerteAsync(source.ClientId, operation.Id, "VigilanceRenforcee", analyse.Niveau.ToString(), analyse.Motif);
        if (analyse.NecessiteDeclaration)
            await _declaration.CreerAlerteAsync(source.ClientId, operation.Id, "DeclarationCNRF", NiveauAlerte.Eleve.ToString(), analyse.Motif);
        if (fractionnement)
            await _declaration.CreerAlerteAsync(source.ClientId, operation.Id, "Fractionnement", NiveauAlerte.Eleve.ToString(),
                "Plusieurs opérations inférieures au seuil cumulées sur 24 heures atteignent le seuil AML.");
        foreach (var alerte in surveillance.Alertes.Where(a => a.Regle is not ("MontantEleve" or "Fractionnement")))
            await _declaration.CreerAlerteAsync(source.ClientId, operation.Id, alerte.Regle, alerte.Niveau.ToString(), alerte.Description);
        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> RejeterAsync(int id, int utilisateurId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var virement = await _context.Virements.FirstOrDefaultAsync(v => v.Id == id);
        if (virement is null) return false;
        if (virement.Statut != "EnAttente")
            throw new InvalidOperationException("Ce virement a déjà été traité.");
        var utilisateur = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (utilisateur is null)
            throw new InvalidOperationException("L'utilisateur connecté est introuvable ou désactivé.");
        var agenceSource = await _context.Comptes.Where(c => c.Id == virement.CompteSourceId).Select(c => c.AgenceId).FirstOrDefaultAsync();
        if (utilisateur.Role != "admin" && agenceSource != utilisateur.AgenceId)
            throw new InvalidOperationException("Ce virement ne relève pas de votre agence.");
        virement.Statut = "Rejete";
        virement.DateValidation = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private async Task<CompteComptable> GetOrCreateLedgerAccountAsync(string code, string label, string currency)
    {
        var account = await _context.ComptesComptables.FirstOrDefaultAsync(c => c.Code == code && c.EstActif);
        if (account is not null) return account;
        account = new CompteComptable { Code = code, Libelle = label, Devise = currency };
        _context.ComptesComptables.Add(account);
        return account;
    }
}
