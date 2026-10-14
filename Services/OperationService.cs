using System.Data;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public class OperationService : IOperationService
{
    private readonly AkarangaBankContext _context;
    private readonly IConformiteService _conformite;
    private readonly ISurveillanceService _surveillance;
    private readonly IDeclarationService _declaration;

    public OperationService(AkarangaBankContext context, IConformiteService conformite,
        ISurveillanceService surveillance, IDeclarationService declaration)
    {
        _context = context;
        _conformite = conformite;
        _surveillance = surveillance;
        _declaration = declaration;
    }

    public async Task<IReadOnlyList<Compte>> GetComptesActifsAsync(int utilisateurId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null) return [];
        var query = _context.Comptes.AsNoTracking().Where(c => c.Statut == "Actif");
        if (user.Role == "client")
        {
            if (!user.ClientId.HasValue) return [];
            query = query.Where(c => c.ClientId == user.ClientId.Value);
        }
        else if (user.Role != "admin") query = query.Where(c => c.AgenceId == user.AgenceId);
        return await query.Include(c => c.Client).OrderBy(c => c.NumeroCompte).ToListAsync();
    }

    public async Task<IReadOnlyList<Operation>> GetRecentAsync(int utilisateurId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null) return [];
        var query = _context.Operations.AsNoTracking();
        if (user.Role == "client")
        {
            if (!user.ClientId.HasValue) return [];
            query = query.Where(o => o.Compte.ClientId == user.ClientId.Value);
        }
        else if (user.Role != "admin") query = query.Where(o => o.AgenceId == user.AgenceId);
        return await query.Include(o => o.Compte).ThenInclude(c => c.Client)
            .Include(o => o.Utilisateur).OrderByDescending(o => o.DateOperation).Take(100).ToListAsync();
    }

    public async Task<Operation> CreateAsync(OperationCreateViewModel vm, int utilisateurId)
    {
        if (vm.Montant <= 0) throw new InvalidOperationException("Le montant doit être supérieur à zéro.");
        if (vm.TypeOperation is not ("Depot" or "Retrait"))
            throw new InvalidOperationException("Le type d'opération doit être un dépôt ou un retrait.");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == vm.CompteId);
        if (user is null || compte is null) throw new InvalidOperationException("L'utilisateur ou le compte est introuvable.");
        if (compte.Statut != "Actif") throw new InvalidOperationException("Le compte doit être actif.");
        if (user.Role == "client" && compte.ClientId != user.ClientId)
            throw new InvalidOperationException("Vous ne pouvez demander une opération que sur votre propre compte.");
        if (user.Role != "client" && user.Role != "admin" && compte.AgenceId != user.AgenceId)
            throw new InvalidOperationException("Le compte n'appartient pas à votre agence.");

        var operation = new Operation
        {
            Reference = ReferenceGenerator.Generer("OPE"),
            TypeOperation = vm.TypeOperation,
            CompteId = compte.Id,
            UtilisateurId = user.Id,
            AgenceId = compte.AgenceId,
            Montant = vm.Montant,
            Devise = compte.Devise,
            Statut = "EnAttente",
            DateOperation = DateTime.UtcNow,
            Commentaire = string.IsNullOrWhiteSpace(vm.Commentaire) ? null : vm.Commentaire.Trim()
        };
        _context.Operations.Add(operation);
        await _context.SaveChangesAsync();
        return operation;
    }

    public async Task<bool> ValiderAsync(int id, int utilisateurId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var operation = await _context.Operations.FirstOrDefaultAsync(o => o.Id == id);
        if (operation is null) return false;
        if (operation.Statut != "EnAttente") throw new InvalidOperationException("Cette opération a déjà été traitée.");
        if (operation.Montant <= 0 || operation.TypeOperation is not ("Depot" or "Retrait"))
            throw new InvalidOperationException("Cette opération contient un montant ou un type invalide.");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == operation.CompteId);
        if (user is null || compte is null) throw new InvalidOperationException("L'utilisateur ou le compte est introuvable.");
        if (user.Role != "admin" && (user.Role == "client" || compte.AgenceId != user.AgenceId))
            throw new InvalidOperationException("Cette opération ne relève pas de votre agence.");
        var analyse = await _conformite.AnalyserTransactionAsync(compte.Id, operation.Montant, operation.TypeOperation);
        if (analyse.EstBloquee) throw new InvalidOperationException(analyse.Motif ?? "Opération bloquée par les contrôles.");
        var dateAnalyse = DateTime.UtcNow;
        var fractionnement = await _conformite.DetecterFractionnementAsync(compte.Id, operation.Montant, dateAnalyse);
        var surveillance = await _surveillance.AnalyserTransactionAsync(compte.Id, operation.Montant, operation.TypeOperation, dateAnalyse);
        if (compte.Statut != "Actif" || compte.Devise != operation.Devise)
            throw new InvalidOperationException("Le compte n'est plus actif ou sa devise a changé.");
        if (operation.TypeOperation == "Retrait" && compte.Solde < operation.Montant)
            throw new InvalidOperationException("Le solde du compte est insuffisant.");

        var now = DateTime.UtcNow;
        compte.Solde += operation.TypeOperation == "Depot" ? operation.Montant : -operation.Montant;
        compte.DateModification = now;
        operation.Statut = "Valide";
        operation.DateValidation = now;
        var codeCaisse = $"CAISSE-{operation.Devise}";
        var caisse = await _context.ComptesComptables.FirstOrDefaultAsync(c => c.Code == codeCaisse && c.EstActif);
        if (caisse is null)
        {
            caisse = new CompteComptable
            {
                Code = codeCaisse,
                Libelle = $"Caisse {operation.Devise}",
                Devise = operation.Devise
            };
            _context.ComptesComptables.Add(caisse);
            await _context.SaveChangesAsync();
        }
        var estDepot = operation.TypeOperation == "Depot";
        _context.Ecritures.AddRange(
            new Ecriture
            {
                OperationId = operation.Id,
                AgenceId = compte.AgenceId,
                CompteId = compte.Id,
                Sens = estDepot ? "Credit" : "Debit",
                Montant = operation.Montant,
                Devise = operation.Devise,
                DateEcriture = now,
                Libelle = $"{operation.TypeOperation} {operation.Reference}"
            },
            new Ecriture
            {
                OperationId = operation.Id,
                AgenceId = compte.AgenceId,
                CompteComptableId = caisse.Id,
                Sens = estDepot ? "Debit" : "Credit",
                Montant = operation.Montant,
                Devise = operation.Devise,
                DateEcriture = now,
                Libelle = $"Contrepartie caisse {operation.Reference}"
            });
        await _context.SaveChangesAsync();
        await CreerAlertesAsync(compte.ClientId, operation.Id, analyse, fractionnement, surveillance);
        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> RejeterAsync(int id, int utilisateurId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var operation = await _context.Operations.FirstOrDefaultAsync(o => o.Id == id);
        if (operation is null) return false;
        if (operation.Statut != "EnAttente") throw new InvalidOperationException("Cette opération a déjà été traitée.");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null) throw new InvalidOperationException("L'utilisateur validateur est introuvable ou désactivé.");
        var agence = await _context.Comptes.Where(c => c.Id == operation.CompteId).Select(c => c.AgenceId).FirstOrDefaultAsync();
        if (user.Role != "admin" && (user.Role == "client" || agence != user.AgenceId))
            throw new InvalidOperationException("Cette opération ne relève pas de votre agence.");
        operation.Statut = "Rejete";
        operation.DateValidation = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private async Task CreerAlertesAsync(int clientId, int operationId, ResultatConformite analyse,
        bool fractionnement, ResultatSurveillance surveillance)
    {
        if (analyse.NecessiteVigilanceRenforcee)
            await _declaration.CreerAlerteAsync(clientId, operationId, "VigilanceRenforcee", analyse.Niveau.ToString(), analyse.Motif);
        if (analyse.NecessiteDeclaration)
            await _declaration.CreerAlerteAsync(clientId, operationId, "DeclarationCNRF", NiveauAlerte.Eleve.ToString(), analyse.Motif);
        if (fractionnement)
            await _declaration.CreerAlerteAsync(clientId, operationId, "Fractionnement", NiveauAlerte.Eleve.ToString(),
                "Plusieurs opérations inférieures au seuil cumulées sur 24 heures atteignent le seuil AML.");
        foreach (var alerte in surveillance.Alertes.Where(a => a.Regle is not ("MontantEleve" or "Fractionnement")))
            await _declaration.CreerAlerteAsync(clientId, operationId, alerte.Regle, alerte.Niveau.ToString(), alerte.Description);
    }
}

