using System.Data;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public class PretService : IPretService
{
    private readonly AkarangaBankContext _context;
    public PretService(AkarangaBankContext context) => _context = context;

    public async Task<IReadOnlyList<Compte>> GetComptesEligiblesAsync(int utilisateurId)
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

    public async Task<IReadOnlyList<Pret>> GetRecentAsync(int utilisateurId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null) return [];
        var query = _context.Prets.AsNoTracking();
        if (user.Role == "client")
        {
            if (!user.ClientId.HasValue) return [];
            query = query.Where(p => p.ClientId == user.ClientId.Value);
        }
        else if (user.Role != "admin") query = query.Where(p => p.CompteDebit.AgenceId == user.AgenceId);
        return await query.Include(p => p.Client).Include(p => p.CompteDebit).Include(p => p.TypePret)
            .OrderByDescending(p => p.DateDemande).Take(100).ToListAsync();
    }

    public async Task<Pret?> GetDetailsAsync(int id, int utilisateurId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null) return null;
        var query = _context.Prets.AsNoTracking().Where(p => p.Id == id);
        if (user.Role == "client")
        {
            if (!user.ClientId.HasValue) return null;
            query = query.Where(p => p.ClientId == user.ClientId.Value);
        }
        else if (user.Role != "admin") query = query.Where(p => p.CompteDebit.AgenceId == user.AgenceId);
        return await query.Include(p => p.Client).Include(p => p.CompteDebit).Include(p => p.TypePret)
            .Include(p => p.Echeances.OrderBy(e => e.NumeroEcheance)).FirstOrDefaultAsync();
    }

    public async Task<Pret> CreateAsync(PretCreateViewModel vm, int utilisateurId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        var account = await _context.Comptes.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == vm.CompteDebitId);
        if (user is null || account is null) throw new InvalidOperationException("L'utilisateur ou le compte est introuvable.");
        if (account.Statut != "Actif") throw new InvalidOperationException("Le compte de versement doit être actif.");
        if (user.Role == "client" && account.ClientId != user.ClientId)
            throw new InvalidOperationException("Vous ne pouvez demander un prêt que pour votre propre compte.");
        if (user.Role != "client" && user.Role != "admin" && account.AgenceId != user.AgenceId)
            throw new InvalidOperationException("Le compte n'appartient pas à votre agence.");
        if (account.Client.Statut != "Actif" || !await _context.VerificationsKYC.AnyAsync(v => v.ClientId == account.ClientId && v.Statut == "Valide"))
            throw new InvalidOperationException("Le client doit être actif et disposer d'une vérification KYC validée.");

        var typePret = await _context.TypesPret.FirstOrDefaultAsync(t => t.Id == vm.TypePretId && t.EstActif);
        if (typePret is null) throw new InvalidOperationException("Le type de prêt sélectionné est indisponible.");
        if (vm.MontantPrincipal < typePret.MontantMin || vm.MontantPrincipal > typePret.MontantMax)
            throw new InvalidOperationException($"Le montant doit être entre {typePret.MontantMin:N2} et {typePret.MontantMax:N2}.");
        if (vm.DureeMois < typePret.DureeMinMois || vm.DureeMois > typePret.DureeMaxMois)
            throw new InvalidOperationException($"La durée doit être entre {typePret.DureeMinMois} et {typePret.DureeMaxMois} mois.");

        var annualRate = (typePret.TauxMin + typePret.TauxMax) / 2m;
        var monthly = CalculerMensualite(vm.MontantPrincipal, annualRate, vm.DureeMois);
        var pret = new Pret
        {
            Reference = $"PRET-{Guid.NewGuid():N}"[..25].ToUpperInvariant(),
            TypePretId = typePret.Id,
            ClientId = account.ClientId,
            CompteDebitId = account.Id,
            MontantPrincipal = vm.MontantPrincipal,
            TauxInteret = annualRate,
            DureeMois = vm.DureeMois,
            Mensualite = monthly,
            Statut = "EnAttente",
            DateDemande = DateTime.UtcNow
            ,Commentaire = vm.Commentaire?.Trim()
        };
        _context.Prets.Add(pret);
        await _context.SaveChangesAsync();
        return pret;
    }

    public async Task<bool> TraiterAsync(int id, int utilisateurId, bool approuver, decimal? tauxPersonnalise = null, string? motifRejet = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var pret = await _context.Prets.FirstOrDefaultAsync(p => p.Id == id);
        if (pret is null) return false;
        if (pret.Statut != "EnAttente") throw new InvalidOperationException("Cette demande a déjà été traitée.");
        if (!approuver && string.IsNullOrWhiteSpace(motifRejet)) throw new InvalidOperationException("Le motif de rejet est obligatoire.");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == pret.CompteDebitId);
        if (user is null || compte is null) throw new InvalidOperationException("L'utilisateur ou le compte est introuvable.");
        if (user.Role != "admin" && (user.Role == "client" || compte.AgenceId != user.AgenceId))
            throw new InvalidOperationException("Cette demande ne relève pas de votre agence.");

        var now = DateTime.UtcNow;
        pret.ValideParId = user.Id;
        pret.DateValidation = now;
        if (!approuver)
        {
            pret.Statut = "Rejete";
            pret.MotifRejet = motifRejet!.Trim();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        if (compte.Statut != "Actif") throw new InvalidOperationException("Le compte de versement n'est plus actif.");

        if (tauxPersonnalise.HasValue)
        {
            if (tauxPersonnalise.Value < 0 || tauxPersonnalise.Value > 100)
                throw new InvalidOperationException("Le taux personnalisé doit être compris entre 0 et 100 %.");
            pret.TauxInteret = tauxPersonnalise.Value;
            pret.Mensualite = CalculerMensualite(pret.MontantPrincipal, pret.TauxInteret, pret.DureeMois);
        }

        pret.Statut = "Valide";
        pret.DateDebut = now.Date;
        pret.DateFin = now.Date.AddMonths(pret.DureeMois);
        foreach (var echeance in GenererEcheancier(pret.MontantPrincipal, pret.TauxInteret, pret.DureeMois, pret.Mensualite, now.Date))
            pret.Echeances.Add(echeance);

        compte.Solde += pret.MontantPrincipal;
        compte.DateModification = now;
        var operation = new Operation
        {
            Reference = $"OP-{Guid.NewGuid():N}"[..27].ToUpperInvariant(),
            TypeOperation = "DeblocagePret",
            CompteId = compte.Id,
            UtilisateurId = user.Id,
            AgenceId = compte.AgenceId,
            PretId = pret.Id,
            Montant = pret.MontantPrincipal,
            Devise = compte.Devise,
            Statut = "Valide",
            DateOperation = now,
            DateValidation = now,
            Commentaire = $"Décaissement du prêt {pret.Reference}"
        };
        _context.Operations.Add(operation);
        await _context.SaveChangesAsync();
        var portefeuille = await _context.ComptesComptables
            .FirstOrDefaultAsync(c => c.Devise == compte.Devise && c.Code == $"PRETS-{compte.Devise}" && c.EstActif);
        if (portefeuille is null)
        {
            portefeuille = new CompteComptable
            {
                Code = $"PRETS-{compte.Devise}",
                Libelle = $"Portefeuille de prêts {compte.Devise}",
                Devise = compte.Devise
            };
            _context.ComptesComptables.Add(portefeuille);
        }
        _context.Ecritures.AddRange(
            new Ecriture
            {
                OperationId = operation.Id,
                AgenceId = compte.AgenceId,
                CompteId = compte.Id,
                Sens = "Credit",
                Montant = pret.MontantPrincipal,
                Devise = compte.Devise,
                DateEcriture = now,
                Libelle = $"Décaissement du prêt {pret.Reference}"
            },
            new Ecriture
            {
                OperationId = operation.Id,
                AgenceId = compte.AgenceId,
                CompteComptable = portefeuille,
                Sens = "Debit",
                Montant = pret.MontantPrincipal,
                Devise = compte.Devise,
                DateEcriture = now,
                Libelle = $"Créance de prêt {pret.Reference}"
            });
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> PayerEcheanceAsync(int echeanceId, int utilisateurId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var echeance = await _context.EcheancesPret.Include(e => e.Pret)
            .ThenInclude(p => p.CompteDebit).FirstOrDefaultAsync(e => e.Id == echeanceId);
        if (echeance is null) return false;
        if (echeance.Statut is not ("EnAttente" or "EnRetard") || echeance.Pret.Statut is not ("Valide" or "Impayé"))
            throw new InvalidOperationException("Cette échéance n'est pas payable.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        var account = echeance.Pret.CompteDebit;
        if (user is null) throw new InvalidOperationException("Utilisateur inactif ou introuvable.");
        if (user.Role == "client" && user.ClientId != echeance.Pret.ClientId ||
            user.Role != "client" && user.Role != "admin" && account.AgenceId != user.AgenceId)
            throw new InvalidOperationException("Cette échéance ne relève pas de votre périmètre.");
        var montantAPayer = echeance.MontantTotal + echeance.Penalite;
        if (account.Statut != "Actif" || account.Solde < montantAPayer)
            throw new InvalidOperationException("Le compte est inactif ou son solde est insuffisant.");

        var now = DateTime.UtcNow;
        var operation = new Operation
        {
            Reference = $"OP-{Guid.NewGuid():N}"[..27].ToUpperInvariant(),
            TypeOperation = "RemboursementPret", CompteId = account.Id,
            UtilisateurId = user.Id, AgenceId = account.AgenceId, PretId = echeance.PretId,
            Montant = montantAPayer, Devise = account.Devise, Statut = "Valide",
            DateOperation = now, DateValidation = now,
            Commentaire = $"Paiement échéance {echeance.NumeroEcheance} du prêt {echeance.Pret.Reference}"
        };
        _context.Operations.Add(operation);
        account.Solde -= montantAPayer;
        echeance.Pret.MontantRembourse += echeance.MontantTotal;
        account.DateModification = now;
        echeance.Statut = "Payee";
        echeance.DatePaiement = now;
        var portfolioCode = $"PRETS-{account.Devise}";
        var incomeCode = $"PRODUITS-INTERETS-{account.Devise}";
        var portfolio = await GetLedgerAccountAsync(portfolioCode, $"Portefeuille de prêts {account.Devise}", account.Devise);
        var income = await GetLedgerAccountAsync(incomeCode, $"Produits d'intérêts {account.Devise}", account.Devise);
        var penaltyIncome = echeance.Penalite > 0
            ? await GetLedgerAccountAsync($"PRODUITS-PENALITES-{account.Devise}", $"Produits de pénalités {account.Devise}", account.Devise)
            : null;
        await _context.SaveChangesAsync();
        _context.Ecritures.AddRange(
            new Ecriture { OperationId = operation.Id, AgenceId = account.AgenceId, CompteId = account.Id, Sens = "Debit", Montant = montantAPayer, Devise = account.Devise, DateEcriture = now, Libelle = operation.Commentaire },
            new Ecriture { OperationId = operation.Id, AgenceId = account.AgenceId, CompteComptable = portfolio, Sens = "Credit", Montant = echeance.Capital, Devise = account.Devise, DateEcriture = now, Libelle = $"Remboursement capital {echeance.Pret.Reference}" },
            new Ecriture { OperationId = operation.Id, AgenceId = account.AgenceId, CompteComptable = income, Sens = "Credit", Montant = echeance.Interet, Devise = account.Devise, DateEcriture = now, Libelle = $"Intérêts prêt {echeance.Pret.Reference}" });
        if (penaltyIncome is not null)
            _context.Ecritures.Add(new Ecriture { OperationId = operation.Id, AgenceId = account.AgenceId, CompteComptable = penaltyIncome, Sens = "Credit", Montant = echeance.Penalite, Devise = account.Devise, DateEcriture = now, Libelle = $"Pénalité prêt {echeance.Pret.Reference}" });
        if (!await _context.EcheancesPret.AnyAsync(e => e.PretId == echeance.PretId && e.Id != echeance.Id && e.Statut != "Payee"))
            echeance.Pret.Statut = "Solde";
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    public async Task<int> MettreAJourRetardsAsync(DateTime? dateReference = null)
    {
        var date = (dateReference ?? DateTime.UtcNow).Date;
        var overdue = await _context.EcheancesPret
            .Include(e => e.Pret)
            .Where(e => e.Statut == "EnAttente" && e.DateEcheance < date &&
                (e.Pret.Statut == "Valide" || e.Pret.Statut == "Impay\u00E9"))
            .ToListAsync();
        foreach (var echeance in overdue)
        {
            echeance.Statut = "EnRetard";
            var moisRetard = Math.Max(1, (date.Year - echeance.DateEcheance.Year) * 12 + date.Month - echeance.DateEcheance.Month);
            echeance.Penalite = Math.Max(500m, decimal.Round(echeance.MontantTotal * 0.005m * moisRetard, 2, MidpointRounding.AwayFromZero));
        }
        var pretIds = overdue.Select(e => e.PretId).Distinct().ToArray();
        foreach (var pretId in pretIds)
        {
            var countAlreadyLate = await _context.EcheancesPret.CountAsync(e => e.PretId == pretId && e.Statut == "EnRetard");
            var count = countAlreadyLate + overdue.Count(e => e.PretId == pretId);
            if (count >= 3)
            {
                var pret = overdue.First(e => e.PretId == pretId).Pret;
                pret.Statut = "Impay\u00E9";
            }
        }
        await _context.SaveChangesAsync();
        return overdue.Count;
    }

    private async Task<CompteComptable> GetLedgerAccountAsync(string code, string libelle, string devise)
    {
        var account = await _context.ComptesComptables.FirstOrDefaultAsync(c => c.Code == code && c.EstActif);
        if (account is not null) return account;
        account = new CompteComptable { Code = code, Libelle = libelle, Devise = devise };
        _context.ComptesComptables.Add(account);
        return account;
    }

    public static decimal CalculerMensualite(decimal principal, decimal tauxAnnuel, int dureeMois)
    {
        if (principal <= 0) throw new ArgumentOutOfRangeException(nameof(principal));
        if (tauxAnnuel < 0 || tauxAnnuel > 100) throw new ArgumentOutOfRangeException(nameof(tauxAnnuel));
        if (dureeMois <= 0) throw new ArgumentOutOfRangeException(nameof(dureeMois));
        var tauxMensuel = tauxAnnuel / 1200m;
        if (tauxMensuel == 0) return decimal.Round(principal / dureeMois, 2, MidpointRounding.AwayFromZero);
        var facteur = (decimal)Math.Pow((double)(1 + tauxMensuel), dureeMois);
        return decimal.Round(principal * tauxMensuel * facteur / (facteur - 1), 2, MidpointRounding.AwayFromZero);
    }

    public static IReadOnlyList<EcheancePret> GenererEcheancier(decimal principal, decimal tauxAnnuel, int dureeMois, decimal mensualite, DateTime dateDebut)
    {
        if (principal <= 0 || tauxAnnuel < 0 || dureeMois <= 0 || mensualite <= 0) throw new ArgumentOutOfRangeException(nameof(principal));
        var result = new List<EcheancePret>(dureeMois);
        var restant = decimal.Round(principal, 2, MidpointRounding.AwayFromZero);
        var tauxMensuel = tauxAnnuel / 1200m;
        for (var numero = 1; numero <= dureeMois; numero++)
        {
            var interet = decimal.Round(restant * tauxMensuel, 2, MidpointRounding.AwayFromZero);
            var capital = numero == dureeMois ? restant : Math.Min(restant, mensualite - interet);
            capital = decimal.Round(capital, 2, MidpointRounding.AwayFromZero);
            var total = capital + interet;
            restant -= capital;
            result.Add(new EcheancePret
            {
                NumeroEcheance = numero, Capital = capital, Interet = interet, MontantTotal = total,
                DateEcheance = dateDebut.Date.AddMonths(numero), Statut = "EnAttente"
            });
        }
        return result;
    }
}
