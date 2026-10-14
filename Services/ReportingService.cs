using AkarangaBank.Web.Data;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public sealed class ReportingService : IReportingService
{
    private readonly AkarangaBankContext _db;
    public ReportingService(AkarangaBankContext db) => _db = db;

    public async Task<IReadOnlyList<JournalReportRow>> JournalAsync(int? agenceId, DateTime? from, DateTime? to, int take = 50000)
    {
        var query = _db.Ecritures.AsNoTracking().Where(e => e.Operation.Statut == "Valide");
        if (agenceId.HasValue) query = query.Where(e => (e.AgenceId ?? e.Operation.AgenceId) == agenceId.Value);
        if (from.HasValue) query = query.Where(e => e.DateEcriture >= from.Value.Date);
        if (to.HasValue) query = query.Where(e => e.DateEcriture < to.Value.Date.AddDays(1));
        return await query.OrderByDescending(e => e.DateEcriture).Take(Math.Clamp(take, 1, 50000))
            .Select(e => new JournalReportRow(e.OperationId, e.DateEcriture, e.Operation.Reference,
                e.Compte != null ? e.Compte.Client.Nom + " " + e.Compte.Client.Prenom : e.Operation.Compte.Client.Nom + " " + e.Operation.Compte.Client.Prenom,
                e.CompteComptable != null ? e.CompteComptable.Code + " - " + e.CompteComptable.Libelle : e.Compte!.NumeroCompte,
                e.Operation.TypeOperation, e.Sens == "Debit" ? e.Montant : 0m,
                e.Sens == "Credit" ? e.Montant : 0m, e.Devise, e.Operation.Statut)).ToListAsync();
    }

    public async Task<IReadOnlyList<BalanceReportRow>> BalanceAsync(int? agenceId, DateTime? from, DateTime? to)
    {
        var query = _db.Ecritures.AsNoTracking().Where(e => e.Operation.Statut == "Valide");
        if (agenceId.HasValue) query = query.Where(e => (e.AgenceId ?? e.Operation.AgenceId) == agenceId.Value);
        if (from.HasValue) query = query.Where(e => e.DateEcriture >= from.Value.Date);
        if (to.HasValue) query = query.Where(e => e.DateEcriture < to.Value.Date.AddDays(1));
        var rows = await query.GroupBy(e => new { e.CompteComptableId, e.CompteId, e.Devise })
            .Select(g => new
            {
                g.Key.CompteComptableId, g.Key.CompteId, g.Key.Devise,
                Debit = g.Sum(e => e.Sens == "Debit" ? e.Montant : 0m),
                Credit = g.Sum(e => e.Sens == "Credit" ? e.Montant : 0m)
            }).ToListAsync();
        var ids = rows.Where(r => r.CompteComptableId.HasValue).Select(r => r.CompteComptableId!.Value).Distinct().ToList();
        var accounts = await _db.ComptesComptables.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
        var clientAccountIds = rows.Where(r => r.CompteId.HasValue).Select(r => r.CompteId!.Value).Distinct().ToList();
        var clientAccounts = await _db.Comptes.AsNoTracking().Where(c => clientAccountIds.Contains(c.Id))
            .Select(c => new { c.Id, c.NumeroCompte }).ToDictionaryAsync(c => c.Id, c => c.NumeroCompte);
        return rows.Select(r =>
        {
            var key = r.CompteComptableId?.ToString() ?? "C" + r.CompteId;
            var label = r.CompteComptableId.HasValue && accounts.TryGetValue(r.CompteComptableId.Value, out var a) ? a.Libelle
                : r.CompteId.HasValue && clientAccounts.TryGetValue(r.CompteId.Value, out var n) ? n : "Compte inconnu";
            var code = r.CompteComptableId.HasValue && accounts.TryGetValue(r.CompteComptableId.Value, out var acct) ? acct.Code : key;
            return new BalanceReportRow(code, label, r.Devise, r.Debit, r.Credit);
        }).OrderBy(r => r.Account).ThenBy(r => r.Currency).ToList();
    }

    public async Task<AmlMonthlyReport> AmlAsync(int? agenceId, DateTime month)
    {
        var start = new DateTime(month.Year, month.Month, 1); var end = start.AddMonths(1);
        var q = _db.AlertesAML.AsNoTracking().Where(a => a.DateAlerte >= start && a.DateAlerte < end);
        if (agenceId.HasValue) q = q.Where(a => a.Client.AgenceId == agenceId.Value);
        var alerts = await q.Select(a => new { a.Niveau, a.TypeAlerte, a.OperationId, Montant = a.Operation == null ? 0m : a.Operation.Montant, a.ClientId, a.Client.NumeroClient, a.Client.Nom, a.Client.Prenom }).ToListAsync();
        var top = alerts.GroupBy(a => new { a.ClientId, a.NumeroClient, a.Nom, a.Prenom })
            .Select(g => new AmlClientReportRow(g.Key.NumeroClient, g.Key.Nom + " " + g.Key.Prenom, g.Count(), g.Sum(a => a.Montant)))
            .OrderByDescending(a => a.AlertCount).ThenByDescending(a => a.SuspiciousAmount).Take(10).ToList();
        return new AmlMonthlyReport(start, alerts.Count, Count("Faible"), Count("Moyen"), Count("Eleve"), Count("Critique"),
            alerts.Count(a => a.TypeAlerte.Contains("CNRF", StringComparison.OrdinalIgnoreCase) || a.TypeAlerte.Contains("DOS", StringComparison.OrdinalIgnoreCase)),
            alerts.Sum(a => a.Montant), top);
        int Count(string level) => alerts.Count(a => string.Equals(a.Niveau, level, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<DashboardReport> DashboardAsync(int? agenceId, DateTime today)
    {
        var clients = _db.Clients.AsNoTracking(); var accounts = _db.Comptes.AsNoTracking(); var operations = _db.Operations.AsNoTracking();
        var loans = _db.Prets.AsNoTracking(); var alerts = _db.AlertesAML.AsNoTracking();
        if (agenceId.HasValue)
        {
            clients = clients.Where(c => c.AgenceId == agenceId.Value); accounts = accounts.Where(c => c.AgenceId == agenceId.Value);
            operations = operations.Where(o => o.AgenceId == agenceId.Value); loans = loans.Where(p => p.Client.AgenceId == agenceId.Value);
            alerts = alerts.Where(a => a.Client.AgenceId == agenceId.Value);
        }
        return new DashboardReport(await clients.CountAsync(), await accounts.CountAsync(), await accounts.Where(c => c.Devise == "BIF").SumAsync(c => (decimal?)c.Solde) ?? 0m,
            await operations.CountAsync(o => o.DateOperation >= today.Date && o.DateOperation < today.Date.AddDays(1)),
            await loans.CountAsync(p => p.Statut == "Valide" && p.MontantRembourse < p.MontantPrincipal),
            await alerts.CountAsync(a => a.Statut == "Ouverte" || a.Statut == "EnCours"));
    }

    public async Task<FinancialStatementReport> FinancialStatementAsync(int? agenceId, DateTime? from, DateTime? to)
    {
        var asOfRows = await BalanceAsync(agenceId, null, to);
        var periodRows = await BalanceAsync(agenceId, from, to);
        var bifAsOf = asOfRows.Where(r => r.Currency == "BIF").ToList();
        var bifPeriod = periodRows.Where(r => r.Currency == "BIF").ToList();
        // Le plan comptable historique du projet utilise des codes descriptifs: les actifs bancaires
        // portent CAISSE/PRETS, les produits PRODUITS, et les charges DEPENSES.
        var revenue = bifPeriod.Where(r => r.Account.StartsWith("PRODUITS", StringComparison.OrdinalIgnoreCase)).Sum(r => r.Credit - r.Debit);
        var expenses = bifPeriod.Where(r => r.Account.StartsWith("DEPENSES", StringComparison.OrdinalIgnoreCase)).Sum(r => r.Debit - r.Credit);
        var assets = bifAsOf.Where(r => r.Account.StartsWith("CAISSE", StringComparison.OrdinalIgnoreCase) || r.Account.StartsWith("PRETS", StringComparison.OrdinalIgnoreCase)).Sum(r => r.Debit - r.Credit);
        var liabilities = bifAsOf.Where(r => r.Account.Length > 1 && r.Account[0] == 'C' && int.TryParse(r.Account[1..], out _)).Sum(r => r.Credit - r.Debit);
        var result = revenue - expenses;
        var equity = assets - liabilities - result;
        return new FinancialStatementReport("Compte de résultat et bilan", to ?? DateTime.Today, assets, liabilities, equity, revenue, expenses, result, asOfRows);
    }

    public async Task<IReadOnlyList<LoanPortfolioRow>> LoanPortfolioAsync(int? agenceId)
    {
        var query = _db.Prets.AsNoTracking().AsQueryable();
        if (agenceId.HasValue) query = query.Where(p => p.Client.AgenceId == agenceId.Value);
        return await query.OrderByDescending(p => p.DateDemande).Select(p => new LoanPortfolioRow(p.Reference,
            p.Client.Nom + " " + p.Client.Prenom, p.TypePret.Libelle, p.DateDebut ?? p.DateDemande,
            p.MontantPrincipal, p.MontantRembourse, p.MontantPrincipal - p.MontantRembourse,
            p.Statut, p.CompteDebit.Devise)).ToListAsync();
    }
}
