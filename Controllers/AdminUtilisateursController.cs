using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminUtilisateursController : Controller
{
    private readonly IUtilisateurService _utilisateurService;
    private readonly IAgenceService _agenceService;

    public AdminUtilisateursController(
        IUtilisateurService utilisateurService,
        IAgenceService agenceService)
    {
        _utilisateurService = utilisateurService;
        _agenceService = agenceService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _utilisateurService.GetAllAsync();
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        await ChargerListesAsync();
        return View(new UtilisateurCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UtilisateurCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(vm.AgenceId);
            return View(vm);
        }

        var (success, error) = await _utilisateurService.CreateAsync(vm);
        if (!success)
        {
            ModelState.AddModelError("", error ?? "Erreur inconnue.");
            await ChargerListesAsync(vm.AgenceId);
            return View(vm);
        }

        TempData["Success"] = "Utilisateur créé avec succès.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActif(int id)
    {
        // Un administrateur ne peut pas désactiver son propre compte
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
        {
            TempData["Error"] = "Vous ne pouvez pas désactiver votre propre compte.";
            return RedirectToAction(nameof(Index));
        }

        await _utilisateurService.ToggleActifAsync(id);
        TempData["Success"] = "Statut modifié.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _utilisateurService.GetByIdAsync(id);
        if (user is null) return NotFound();

        return View(new UtilisateurResetPasswordViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(UtilisateurResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (success, error) = await _utilisateurService.ResetPasswordAsync(vm.Id, vm.NewPassword);
        if (!success)
        {
            ModelState.AddModelError("", error ?? "Erreur inconnue.");
            return View(vm);
        }

        TempData["Success"] = "Mot de passe réinitialisé.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ChargerListesAsync(int? selectedId = null)
    {
        var agencesVm = await _agenceService.GetPagedAsync(null, 1, int.MaxValue);
        ViewBag.Agences = new SelectList(
            agencesVm.Agences.Select(a => new { a.Id, Libelle = $"{a.Code} — {a.Ville}" }),
            "Id", "Libelle", selectedId);

        ViewBag.Roles = new SelectList(new[]
        {
            "admin", "gestionnaire", "auditeur", "guichetier", "client"
        });
    }
}
