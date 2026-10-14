using System.Security.Claims;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire,auditeur")]
public sealed class ReportingController : Controller
{
    private readonly IReportingService _reports;
    private readonly ReportingExportService _exports;
    private readonly AkarangaBankContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ReportingPdfService _pdf;
    private readonly IComptabiliteService _comptabilite;
    public ReportingController(IReportingService reports, ReportingExportService exports, ReportingPdfService pdf, IComptabiliteService comptabilite, AkarangaBankContext db, UserManager<ApplicationUser> users)
    { _reports = reports; _exports = exports; _pdf = pdf; _comptabilite = comptabilite; _db = db; _users = users; }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        ViewBag.Dashboard = await _reports.DashboardAsync(await AgencyScopeAsync(), today);
        return View();
    }

    public async Task<IActionResult> Journal(DateTime? du, DateTime? au, int page = 1)
    {
        if (du.HasValue && au.HasValue && du > au) ModelState.AddModelError(string.Empty, "La date de début doit précéder la date de fin.");
        var rows = await _reports.JournalAsync(await AgencyScopeAsync(), du, au, 50000);
        ViewBag.Du = du?.ToString("yyyy-MM-dd"); ViewBag.Au = au?.ToString("yyyy-MM-dd"); ViewBag.Page = Math.Max(page, 1);
        ViewBag.Total = rows.Count;
        return View(rows.Skip((Math.Max(page, 1) - 1) * 50).Take(50).ToList());
    }

    public async Task<IActionResult> Balance(DateTime? du, DateTime? au)
    {
        var rows = await _reports.BalanceAsync(await AgencyScopeAsync(), du, au);
        ViewBag.TotalsByCurrency = rows.GroupBy(r => r.Currency).Select(g => new { Currency = g.Key, Debit = g.Sum(r => r.Debit), Credit = g.Sum(r => r.Credit) }).ToList();
        return View(rows);
    }

    public async Task<IActionResult> EtatsFinanciers(DateTime? du, DateTime? au) =>
        View(await _reports.FinancialStatementAsync(await AgencyScopeAsync(), du, au));

    public async Task<IActionResult> Aml(int? annee, int? mois)
    {
        var month = new DateTime(annee ?? DateTime.Today.Year, Math.Clamp(mois ?? DateTime.Today.Month, 1, 12), 1);
        return View(await _reports.AmlAsync(await AgencyScopeAsync(), month));
    }

    [HttpGet]
    public async Task<IActionResult> ExportJournalExcel(DateTime? du, DateTime? au)
    {
        var rows = await _reports.JournalAsync(await AgencyScopeAsync(), du, au);
        await AuditAsync("ExportExcelJournal", $"Lignes exportées: {rows.Count}");
        return File(_exports.JournalExcel(rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"journal-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportJournalCsv(DateTime? du, DateTime? au)
    {
        var rows = await _reports.JournalAsync(await AgencyScopeAsync(), du, au);
        await AuditAsync("ExportCsvJournal", $"Lignes exportées: {rows.Count}");
        return File(_exports.CsvJournal(rows), "text/csv; charset=utf-8", $"journal-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportBalanceExcel(DateTime? du, DateTime? au)
    {
        var rows = await _reports.BalanceAsync(await AgencyScopeAsync(), du, au);
        await AuditAsync("ExportExcelBalance", $"Comptes exportés: {rows.Count}");
        return File(_exports.BalanceExcel(rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"balance-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportAmlExcel(int? annee, int? mois)
    {
        var month = new DateTime(annee ?? DateTime.Today.Year, Math.Clamp(mois ?? DateTime.Today.Month, 1, 12), 1);
        var report = await _reports.AmlAsync(await AgencyScopeAsync(), month);
        await AuditAsync("ExportExcelAML", $"Mois: {month:yyyy-MM}; alertes: {report.TotalAlerts}");
        return File(_exports.AmlExcel(report), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"aml-{month:yyyy-MM}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPortfolioPretsExcel()
    {
        var rows = await _reports.LoanPortfolioAsync(await AgencyScopeAsync());
        await AuditAsync("ExportExcelPortefeuillePrets", $"Prêts exportés: {rows.Count}");
        return File(_exports.LoanPortfolioExcel(rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"portefeuille-prets-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> RelevePdf(int compteId)
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        var statement = await _comptabilite.GetReleveAsync(compteId, userId);
        if (statement is null) return NotFound();
        var lines = statement.Ecritures.Select(e => (IReadOnlyList<string>)[e.DateEcriture.ToString("dd/MM/yyyy HH:mm"), e.Operation.Reference,
            e.Operation.TypeOperation, e.Sens == "Credit" ? "Crédit" : "Débit", e.Montant.ToString("N2"), e.Devise, e.Libelle ?? ""])
            .Prepend((IReadOnlyList<string>)["", "", "Solde initial", "", statement.SoldeInitial.ToString("N2"), statement.Compte.Devise, ""])
            .Append((IReadOnlyList<string>)["", "", "Solde final", "", statement.SoldeFinal.ToString("N2"), statement.Compte.Devise, ""])
            .ToList();
        await AuditAsync("GenerationRelevePDF", $"Compte {statement.Compte.NumeroCompte}");
        var bytes = _pdf.Generate("Relevé de compte", $"Client: {statement.Compte.Client.Nom} {statement.Compte.Client.Prenom} — Compte: {statement.Compte.NumeroCompte}",
            ["Date", "Référence", "Opération", "Sens", "Montant", "Devise", "Libellé"], lines);
        return File(bytes, "application/pdf", $"releve-{statement.Compte.NumeroCompte}-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> RecuOperationPdf(int operationId)
    {
        var operation = await _db.Operations.AsNoTracking().Include(o => o.Compte).ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(o => o.Id == operationId);
        if (operation is null || await _comptabilite.GetReleveAsync(operation.CompteId,
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0) is null) return NotFound();
        await AuditAsync("GenerationRecuPDF", $"Opération {operation.Reference}");
        var pdfRows = new List<IReadOnlyList<string>>
        {
            new[] { "Référence", operation.Reference }, new[] { "Date", operation.DateOperation.ToString("dd/MM/yyyy HH:mm") },
            new[] { "Client", operation.Compte.Client.Nom + " " + operation.Compte.Client.Prenom }, new[] { "Compte", operation.Compte.NumeroCompte },
            new[] { "Type", operation.TypeOperation }, new[] { "Montant", operation.Montant.ToString("N2") + " " + operation.Devise }, new[] { "Statut", operation.Statut }
        };
        var bytes = _pdf.Generate("Reçu d'opération", "Justificatif à conserver", ["Information", "Valeur"], pdfRows, a5: true);
        return File(bytes, "application/pdf", $"recu-{operation.Reference}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> AttestationSoldePdf(int compteId)
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        var statement = await _comptabilite.GetReleveAsync(compteId, userId);
        if (statement is null) return NotFound();
        var words = ReportingPdfService.MoneyInWords(statement.SoldeFinal, statement.Compte.Devise);
        var rows = new List<IReadOnlyList<string>>
        {
            new[] { "Titulaire", statement.Compte.Client.Nom + " " + statement.Compte.Client.Prenom },
            new[] { "Compte", statement.Compte.NumeroCompte }, new[] { "Solde au " + DateTime.Today.ToString("dd/MM/yyyy"), statement.SoldeFinal.ToString("N2") + " " + statement.Compte.Devise },
            new[] { "Montant en lettres", words }, new[] { "Fait le", DateTime.Today.ToString("dd/MM/yyyy") }, new[] { "Signature et cachet", "_______________________________" }
        };
        await AuditAsync("GenerationAttestationSoldePDF", $"Compte {statement.Compte.NumeroCompte}");
        return File(_pdf.Generate("Attestation de solde", "Document délivré à la demande du titulaire", ["Information", "Valeur"], rows),
            "application/pdf", $"attestation-solde-{statement.Compte.NumeroCompte}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ContratPretPdf(int pretId)
    {
        var query = _db.Prets.AsNoTracking().Include(p => p.Client).Include(p => p.TypePret).AsQueryable();
        var scope = await AgencyScopeAsync();
        if (scope.HasValue) query = query.Where(p => p.Client.AgenceId == scope.Value);
        var loan = await query.FirstOrDefaultAsync(p => p.Id == pretId);
        if (loan is null) return NotFound();
        var rows = new List<IReadOnlyList<string>>
        {
            new[] { "Référence", loan.Reference }, new[] { "Emprunteur", loan.Client.Nom + " " + loan.Client.Prenom + " — " + loan.Client.NumeroClient },
            new[] { "Type de prêt", loan.TypePret.Libelle }, new[] { "Principal", loan.MontantPrincipal.ToString("N2") },
            new[] { "Taux annuel", loan.TauxInteret.ToString("N2") + "%" }, new[] { "Durée", loan.DureeMois + " mois" },
            new[] { "Mensualité", loan.Mensualite.ToString("N2") }, new[] { "Remboursement", "Selon le tableau d'échéances annexé. Les échéances impayées peuvent entraîner les pénalités prévues par la convention et la réglementation applicable." },
            new[] { "Engagement", "L'emprunteur autorise le remboursement selon l'échéancier convenu et s'engage à signaler tout changement de coordonnées." },
            new[] { "Signatures", "Emprunteur: ____________________   Banque: ____________________" }
        };
        await AuditAsync("GenerationContratPretPDF", $"Prêt {loan.Reference}");
        return File(_pdf.Generate("Contrat de prêt", "Projet de contrat — vérifier et signer les conditions particulières", ["Article / élément", "Conditions"], rows),
            "application/pdf", $"contrat-pret-{loan.Reference}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> DeclarationCnrFPdf(int alerteId)
    {
        var query = _db.AlertesAML.AsNoTracking().Include(a => a.Client).Include(a => a.Operation).AsQueryable();
        var scope = await AgencyScopeAsync();
        if (scope.HasValue) query = query.Where(a => a.Client.AgenceId == scope.Value);
        var alert = await query.FirstOrDefaultAsync(a => a.Id == alerteId);
        if (alert is null) return NotFound();
        var rows = new List<IReadOnlyList<string>>
        {
            new[] { "Référence interne", alert.Id.ToString() }, new[] { "Client", alert.Client.NumeroClient + " — " + alert.Client.Nom + " " + alert.Client.Prenom },
            new[] { "Opération", alert.Operation?.Reference ?? "Non rattachée" }, new[] { "Montant", alert.Operation is null ? "Non renseigné" : alert.Operation.Montant.ToString("N2") + " " + alert.Operation.Devise },
            new[] { "Motif / description", alert.Description ?? alert.TypeAlerte }, new[] { "Niveau", alert.Niveau }, new[] { "Date de détection", alert.DateAlerte.ToString("dd/MM/yyyy HH:mm") },
            new[] { "Référence réglementaire", "Déclaration préparatoire à examiner selon les obligations nationales de lutte contre le blanchiment." },
            new[] { "Visa conformité", "Nom, qualité, date et signature: ____________________" }
        };
        await AuditAsync("GenerationDeclarationCnrFPDF", $"Alerte {alert.Id}");
        return File(_pdf.Generate("Déclaration CNRF — dossier préparatoire", "Confidentiel — validation conformité requise", ["Rubrique", "Donnée"], rows),
            "application/pdf", $"cnrf-dossier-{alert.Id}.pdf");
    }

    [HttpGet]
    public IActionResult Imprimer() => View("Print");

    private async Task<int?> AgencyScopeAsync()
    {
        if (User.IsInRole("admin")) return null;
        var user = await _users.GetUserAsync(User);
        return user?.AgenceId ?? throw new UnauthorizedAccessException("Agence utilisateur introuvable.");
    }

    private async Task AuditAsync(string action, string details)
    {
        var user = await _users.GetUserAsync(User);
        _db.AuditLogs.Add(new AuditLog
        {
            UtilisateurId = user?.Id, AgenceId = user?.AgenceId, Action = action,
            TableConcernee = "Reporting", DateEvenement = DateTime.UtcNow,
            AdresseIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(), Details = details
        });
        await _db.SaveChangesAsync();
    }
}
