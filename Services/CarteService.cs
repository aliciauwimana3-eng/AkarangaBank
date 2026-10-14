using System.Data;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public class CarteService(AkarangaBankContext db, ISurveillanceService surveillance, IDeclarationService declarations, CartePanProtection panProtection) : ICarteService
{
    public async Task<TransactionCarte> AutoriserTransactionAsync(int carteId, int utilisateurId, decimal montant, string typeTransaction)
    {
        if (montant <= 0) throw new ArgumentOutOfRangeException(nameof(montant), "Le montant doit être supérieur à zéro.");
        if (typeTransaction is not ("Retrait" or "Paiement" or "EnLigne")) throw new ArgumentException("Type de transaction invalide.", nameof(typeTransaction));

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var carte = await db.Cartes.Include(c => c.Compte).ThenInclude(c => c.Client).FirstOrDefaultAsync(c => c.Id == carteId);
        if (carte is null) throw new InvalidOperationException("Carte introuvable.");
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null || user.Role == "client" && user.ClientId != carte.ClientId ||
            user.Role != "client" && user.Role != "admin" && user.AgenceId != carte.Compte.AgenceId)
            throw new UnauthorizedAccessException("Cette carte ne relève pas de votre périmètre.");

        string? motif = null;
        var compte = carte.Compte;
        var today = DateTime.UtcNow.Date;
        if (carte.Statut != "Active") motif = "La carte n'est pas active.";
        else if (carte.DateExpiration.Date < today) motif = "La carte est expirée.";
        else if (compte.Statut != "Actif" || compte.Solde < montant) motif = "Compte inactif ou solde insuffisant.";
        var plafond = typeTransaction switch
        {
            "Retrait" => carte.PlafondRetraitJournalier,
            "EnLigne" => carte.PlafondEnLigneJournalier,
            _ => carte.PlafondPaiementJournalier
        };
        var dejaConsomme = await db.TransactionsCartes.Where(t => t.CarteId == carteId && t.Statut == "Autorisee" &&
            t.TypeTransaction == typeTransaction && t.DateTransaction >= today).SumAsync(t => (decimal?)t.Montant) ?? 0m;
        if (motif is null && dejaConsomme + montant > plafond) motif = "Le plafond journalier de cette catégorie serait dépassé.";

        ResultatSurveillance? analyse = null;
        if (motif is null)
        {
            analyse = await surveillance.AnalyserTransactionAsync(compte.Id, montant, $"Carte{typeTransaction}", DateTime.UtcNow);
            if (analyse.Alertes.Count > 0) motif = "Transaction soumise à une vérification AML.";
        }
        var transaction = new TransactionCarte
        {
            CarteId = carte.Id, Montant = montant, TypeTransaction = typeTransaction,
            Statut = motif is null ? "Autorisee" : "Refusee", MotifRefus = motif, DateTransaction = DateTime.UtcNow
        };
        db.TransactionsCartes.Add(transaction);
        if (motif is not null)
        {
            await db.SaveChangesAsync();
            if (analyse?.NecessiteDeclaration == true)
                await declarations.CreerAlerteAsync(carte.ClientId, null, "TransactionCarteAML", "Eleve", motif);
            await tx.CommitAsync();
            return transaction;
        }

        var now = DateTime.UtcNow;
        compte.Solde -= montant;
        compte.DateModification = now;
        var operation = new Operation
        {
            Reference = $"OP-{Guid.NewGuid():N}"[..27].ToUpperInvariant(), TypeOperation = "TransactionCarte",
            CompteId = compte.Id, UtilisateurId = user!.Id, AgenceId = compte.AgenceId, Montant = montant,
            Devise = compte.Devise, Statut = "Valide", DateOperation = now, DateValidation = now,
            Commentaire = $"{typeTransaction} carte {carte.NumeroMasque}"
        };
        db.Operations.Add(operation);
        await db.SaveChangesAsync();
        transaction.OperationId = operation.Id;
        var expense = await db.ComptesComptables.FirstOrDefaultAsync(c => c.Code == $"DEPENSES-CARTE-{compte.Devise}" && c.EstActif);
        if (expense is null)
        {
            expense = new CompteComptable { Code = $"DEPENSES-CARTE-{compte.Devise}", Libelle = $"Règlements par carte {compte.Devise}", Devise = compte.Devise };
            db.ComptesComptables.Add(expense);
        }
        db.Ecritures.AddRange(
            new Ecriture { OperationId = operation.Id, AgenceId = compte.AgenceId, CompteId = compte.Id, Sens = "Debit", Montant = montant, Devise = compte.Devise, DateEcriture = now, Libelle = operation.Commentaire },
            new Ecriture { OperationId = operation.Id, AgenceId = compte.AgenceId, CompteComptable = expense, Sens = "Credit", Montant = montant, Devise = compte.Devise, DateEcriture = now, Libelle = operation.Commentaire });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return transaction;
    }

    public async Task<IReadOnlyList<TransactionCarte>> GetHistoriqueAsync(int carteId, int utilisateurId)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        var query = db.TransactionsCartes.AsNoTracking().Include(t => t.Operation).Where(t => t.CarteId == carteId);
        query = query.Where(t => user != null && (user.Role == "admin" || user.Role == "client" && user.ClientId == t.Carte.ClientId ||
            user.Role != "client" && t.Carte.Compte.AgenceId == user.AgenceId));
        return await query.OrderByDescending(t => t.DateTransaction).Take(250).ToListAsync();
    }

    public async Task BloquerAsync(int carteId, int utilisateurId, string motif)
    {
        var carte = await db.Cartes.Include(c => c.Compte).FirstOrDefaultAsync(c => c.Id == carteId);
        if (carte is null) throw new InvalidOperationException("Carte introuvable.");
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null || user.Role != "admin" && user.Role != "gestionnaire" && (user.Role != "client" || user.ClientId != carte.ClientId))
            throw new UnauthorizedAccessException("Vous ne pouvez pas bloquer cette carte.");
        if (motif is not ("Perte" or "Vol" or "Fraude" or "Autre")) throw new ArgumentException("Motif d'opposition invalide.");
        carte.Statut = motif switch { "Perte" => "Perdue", "Vol" => "Volee", "Fraude" => "Opposition", _ => "Bloquee" };
        carte.MotifBlocage = motif;
        await db.SaveChangesAsync();
    }

    public async Task RenouvelerAsync(int carteId, int utilisateurId)
    {
        var old = await db.Cartes.Include(c => c.Compte).FirstOrDefaultAsync(c => c.Id == carteId);
        if (old is null) throw new InvalidOperationException("Carte introuvable.");
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null || user.Role != "admin" && user.Role != "gestionnaire" && (user.Role != "client" || user.ClientId != old.ClientId))
            throw new UnauthorizedAccessException("Vous ne pouvez pas renouveler cette carte.");
        if (old.Statut != "Active") throw new InvalidOperationException("Seule une carte active peut être renouvelée.");
        old.Statut = "Expiree";
        var pan = GenererNumero(old.TypeCarte);
        var masked = $"**** **** **** {pan[^4..]}";
        while (await db.Cartes.AnyAsync(c => c.NumeroCarte == masked))
        {
            pan = GenererNumero(old.TypeCarte);
            masked = $"**** **** **** {pan[^4..]}";
        }
        var replacement = new Carte
        {
            NumeroCarte = masked, CompteId = old.CompteId, ClientId = old.ClientId,
            DerniersChiffres = pan[^4..], NumeroCarteChiffre = panProtection.Proteger(pan),
            TypeCarte = old.TypeCarte, DateExpiration = DateTime.UtcNow.Date.AddYears(3), Statut = "Active",
            PlafondRetraitJournalier = old.PlafondRetraitJournalier, PlafondPaiementJournalier = old.PlafondPaiementJournalier,
            PlafondEnLigneJournalier = old.PlafondEnLigneJournalier
        };
        db.Cartes.Add(replacement);
        await db.SaveChangesAsync();
    }

    public static string GenererNumero(string type)
    {
        var prefix = type switch { "Visa" => "4", "MasterCard" => "5", "Locale" => "6", _ => throw new ArgumentException("Type de carte invalide.") };
        var digits = prefix + string.Concat(Enumerable.Range(0, 14).Select(_ => System.Security.Cryptography.RandomNumberGenerator.GetInt32(10)));
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[i] - '0';
            if (i % 2 == 0 && (digit *= 2) > 9) digit -= 9;
            sum += digit;
        }
        digits = digits[..15] + ((10 - sum % 10) % 10).ToString();
        return digits;
    }
}
