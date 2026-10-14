using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

public class OperationsController : Controller
{
    private readonly IOperationService _service;
    public OperationsController(IOperationService service) => _service = service;

    public async Task<IActionResult> Index() => View(new OperationListViewModel
    {
        Operations = await _service.GetRecentAsync(UtilisateurId()),
        PeutValider = User.IsInRole("admin") || User.IsInRole("gestionnaire")
    });

    public async Task<IActionResult> Create()
    {
        await ChargerComptesAsync();
        return View(new OperationCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OperationCreateViewModel vm)
    {
        if (!ModelState.IsValid) { await ChargerComptesAsync(vm.CompteId); return View(vm); }
        try
        {
            await _service.CreateAsync(vm, UtilisateurId());
            TempData["Success"] = "Opération enregistrée et envoyée en attente de validation.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GestionnaireOrAdmin"), Authorize(Policy = "RequireMfa")]
    public Task<IActionResult> Valider(int id) => TraiterAsync(id, true);

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GestionnaireOrAdmin"), Authorize(Policy = "RequireMfa")]
    public Task<IActionResult> Rejeter(int id) => TraiterAsync(id, false);

    private async Task<IActionResult> TraiterAsync(int id, bool valider)
    {
        try
        {
            var ok = valider ? await _service.ValiderAsync(id, UtilisateurId()) : await _service.RejeterAsync(id, UtilisateurId());
            if (!ok) return NotFound();
            TempData["Success"] = valider ? "Opération validée et comptabilisée." : "Opération rejetée.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private int UtilisateurId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new InvalidOperationException("Impossible d'identifier l'utilisateur connecté.");

    private async Task ChargerComptesAsync(int? compteId = null)
    {
        var comptes = await _service.GetComptesActifsAsync(UtilisateurId());
        ViewBag.Comptes = new SelectList(comptes.Select(c => new
        {
            c.Id,
            Libelle = $"{c.NumeroCompte} — {c.Client.Nom} {c.Client.Prenom} ({c.Solde:N2} {c.Devise})"
        }), "Id", "Libelle", compteId);
    }
}
