using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using AkarangaBank.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

public class VirementsController : Controller
{
    private readonly IVirementService _service;
    private readonly AkarangaBankContext _db;
    private readonly INotificationService _notifications;

    public VirementsController(IVirementService service, AkarangaBankContext db, INotificationService notifications)
    {
        _service = service;
        _db = db;
        _notifications = notifications;
    }

    public async Task<IActionResult> Index()
    {
        var peutValider = User.IsInRole("admin") || User.IsInRole("gestionnaire");
        return View(new VirementListViewModel
        {
            Virements = await _service.GetRecentAsync(UtilisateurId()),
            PeutValider = peutValider
        });
    }

    public async Task<IActionResult> Create()
    {
        await ChargerComptesAsync();
        return View(new VirementCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VirementCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }

        try
        {
            await _service.CreateAsync(vm, UtilisateurId());
            TempData["Success"] = "Virement enregistré et envoyé en attente de validation.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "GestionnaireOrAdmin")]
    [Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> Valider(int id) => await TraiterAsync(id, true);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "GestionnaireOrAdmin")]
    [Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> Rejeter(int id) => await TraiterAsync(id, false);

    private async Task<IActionResult> TraiterAsync(int id, bool valider)
    {
        try
        {
            var ok = valider
                ? await _service.ValiderAsync(id, UtilisateurId())
                : await _service.RejeterAsync(id, UtilisateurId());
            if (!ok) return NotFound();
            if (valider)
            {
                var transfer = await _db.Virements.AsNoTracking().Where(v => v.Id == id)
                    .Select(v => new { v.Reference, v.Montant, v.Devise, ClientId = v.CompteDestination!.ClientId }).FirstOrDefaultAsync();
                if (transfer is not null)
                {
                    var recipients = await _db.Users.AsNoTracking().Where(u => u.EstActif && u.ClientId == transfer.ClientId && u.Email != null).Select(u => u.Id).ToListAsync();
                    foreach (var recipient in recipients)
                    {
                        try
                        {
                            await _notifications.CreerEtEnvoyerAsync(recipient, "Email", "VIREMENT_RECU", "fr-BI", new Dictionary<string, string>
                            {
                                ["Montant"] = transfer.Montant.ToString("N0"), ["Devise"] = transfer.Devise, ["Reference"] = transfer.Reference
                            });
                        }
                        catch (Exception notificationError)
                        {
                            HttpContext.RequestServices.GetRequiredService<ILogger<VirementsController>>().LogError(notificationError, "Impossible de planifier la notification du virement {Reference}.", transfer.Reference);
                        }
                    }
                }
            }
            TempData["Success"] = valider ? "Virement approuvé et comptabilisé." : "Virement rejeté.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private int UtilisateurId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("Impossible d'identifier l'utilisateur connecté.");

    private async Task ChargerComptesAsync(int? sourceId = null, int? destinationId = null)
    {
        var comptes = await _service.GetComptesActifsAsync(UtilisateurId());
        static string Libelle(AkarangaBank.Web.Models.Compte c) =>
            $"{c.NumeroCompte} — {c.Client.Nom} {c.Client.Prenom} ({c.Solde:N2} {c.Devise})";
        ViewBag.ComptesSource = new SelectList(comptes.Select(c => new { c.Id, Libelle = Libelle(c) }), "Id", "Libelle", sourceId);
        ViewBag.ComptesDestination = new SelectList(comptes.Select(c => new { c.Id, Libelle = Libelle(c) }), "Id", "Libelle", destinationId);
    }
}
