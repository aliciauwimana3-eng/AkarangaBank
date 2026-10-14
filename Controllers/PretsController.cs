using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

public class PretsController : Controller
{
    private readonly IPretService _service;
    private readonly AkarangaBankContext _context;
    public PretsController(IPretService service, AkarangaBankContext context) { _service = service; _context = context; }

    public async Task<IActionResult> Index() => View(new PretListViewModel
    {
        Prets = await _service.GetRecentAsync(UtilisateurId()),
        PeutValider = User.IsInRole("admin") || User.IsInRole("gestionnaire")
    });

    public async Task<IActionResult> Details(int id)
    {
        var pret = await _service.GetDetailsAsync(id, UtilisateurId());
        return pret is null ? NotFound() : View(pret);
    }

    [Authorize(Policy = "GuichetierOnly")]
    public async Task<IActionResult> Create()
    {
        await ChargerComptesAsync();
        await ChargerTypesPretAsync();
        return View(new PretCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GuichetierOnly")]
    public async Task<IActionResult> Create(PretCreateViewModel vm)
    {
        if (!ModelState.IsValid) { await ChargerComptesAsync(vm.CompteDebitId); await ChargerTypesPretAsync(vm.TypePretId); return View(vm); }
        try
        {
            await _service.CreateAsync(vm, UtilisateurId());
            TempData["Success"] = "Demande de prêt enregistrée, en attente de validation.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesAsync(vm.CompteDebitId);
            await ChargerTypesPretAsync(vm.TypePretId);
            return View(vm);
        }
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GestionnaireOrAdmin"), Authorize(Policy = "RequireMfa")]
    public Task<IActionResult> Approuver(int id, decimal? tauxPersonnalise) => TraiterAsync(id, true, tauxPersonnalise, null);

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GestionnaireOrAdmin"), Authorize(Policy = "RequireMfa")]
    public Task<IActionResult> Rejeter(int id, string motifRejet) => TraiterAsync(id, false, null, motifRejet);

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GestionnaireOrAdmin"), Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> MettreAJourRetards()
    {
        var count = await _service.MettreAJourRetardsAsync();
        TempData["Success"] = $"Mise à jour terminée : {count} échéance(s) passée(s) en retard.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GestionnaireOrAdmin"), Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> PayerEcheance(int id)
    {
        try
        {
            if (!await _service.PayerEcheanceAsync(id, UtilisateurId())) return NotFound();
            TempData["Success"] = "Échéance remboursée et comptabilisée.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> TraiterAsync(int id, bool approuver, decimal? tauxPersonnalise, string? motifRejet)
    {
        try
        {
            var ok = await _service.TraiterAsync(id, UtilisateurId(), approuver, tauxPersonnalise, motifRejet);
            if (!ok) return NotFound();
            if (approuver)
            {
                var loan = await _context.Prets.AsNoTracking().Where(p => p.Id == id)
                    .Select(p => new { p.Reference, p.MontantPrincipal, p.ClientId, Nom = p.Client.Nom, Prenom = p.Client.Prenom }).FirstOrDefaultAsync();
                if (loan is not null)
                {
                    var recipients = await _context.Users.AsNoTracking().Where(u => u.EstActif && u.ClientId == loan.ClientId && u.Email != null).Select(u => u.Id).ToListAsync();
                    foreach (var recipient in recipients)
                    {
                        try
                        {
                            await HttpContext.RequestServices.GetRequiredService<INotificationService>().CreerEtEnvoyerAsync(recipient, "Email", "PRET_VALIDE", "fr-BI", new Dictionary<string, string>
                            {
                                ["Reference"] = loan.Reference, ["Montant"] = loan.MontantPrincipal.ToString("N0"), ["NomClient"] = $"{loan.Prenom} {loan.Nom}"
                            });
                        }
                        catch (Exception notificationError)
                        {
                            HttpContext.RequestServices.GetRequiredService<ILogger<PretsController>>().LogError(notificationError, "Impossible de planifier la notification du pret {Reference}.", loan.Reference);
                        }
                    }
                }
            }
            TempData["Success"] = approuver ? "Prêt approuvé, échéancier créé et fonds versés." : "Demande de prêt rejetée.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private int UtilisateurId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new InvalidOperationException("Impossible d'identifier l'utilisateur connecté.");

    private async Task ChargerComptesAsync(int? selected = null)
    {
        var comptes = await _service.GetComptesEligiblesAsync(UtilisateurId());
        ViewBag.Comptes = new SelectList(comptes.Select(c => new
        {
            c.Id,
            Libelle = $"{c.NumeroCompte} — {c.Client.Nom} {c.Client.Prenom} ({c.Devise})"
        }), "Id", "Libelle", selected);
    }

    private async Task ChargerTypesPretAsync(int? selected = null)
    {
        var types = await _context.TypesPret.AsNoTracking().Where(t => t.EstActif).OrderBy(t => t.Libelle).ToListAsync();
        ViewBag.TypesPret = new SelectList(types.Select(t => new
        {
            t.Id,
            Libelle = $"{t.Libelle} — {t.MontantMin:N0} à {t.MontantMax:N0} BIF, {t.DureeMinMois}-{t.DureeMaxMois} mois, taux moyen {(t.TauxMin + t.TauxMax) / 2m:N2}%"
        }), "Id", "Libelle", selected);
    }
}
