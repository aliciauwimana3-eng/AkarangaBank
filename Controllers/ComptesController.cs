using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire,guichetier,client")]
public class ComptesController : Controller
{
    private readonly ICompteService _compteService;
    private readonly IClientService _clientService;
    private readonly IAgenceService _agenceService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ComptesController(
        ICompteService compteService,
        IClientService clientService,
        IAgenceService agenceService,
        UserManager<ApplicationUser> userManager)
    {
        _compteService = compteService;
        _clientService = clientService;
        _agenceService = agenceService;
        _userManager = userManager;
    }

    // Retourne null pour le personnel (voit tout).
    // Pour un client : son ClientId, ou -1 s'il n'est lié à aucune fiche (donc aucun compte visible).
    private async Task<int?> ClientIdRestreintAsync()
    {
        if (!User.IsInRole("client")) return null;
        var user = await _userManager.GetUserAsync(User);
        return user?.ClientId ?? -1;
    }

    // GET: /Comptes
    public async Task<IActionResult> Index(
        string? recherche, int? agenceId, int? typeCompteId, string? statut, int page = 1)
    {
        const int taille = 10;
        var clientId = await ClientIdRestreintAsync();
        var vm = await _compteService.GetPagedAsync(
            recherche, agenceId, typeCompteId, statut, page, taille, clientId);
        return View(vm);
    }

    // GET: /Comptes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _compteService.GetDetailsAsync(id);
        if (vm is null) return NotFound();

        // Un client ne peut ouvrir que ses propres comptes
        var clientId = await ClientIdRestreintAsync();
        if (clientId.HasValue && vm.Compte.ClientId != clientId.Value)
            return NotFound();

        return View(vm);
    }

    // GET: /Comptes/Create?clientId=5
    [Authorize(Roles = "admin,gestionnaire,guichetier")]
    public async Task<IActionResult> Create(int? clientId)
    {
        await ChargerListesAsync(clientId);
        return View(new CompteCreateViewModel { ClientId = clientId ?? 0 });
    }

    // POST: /Comptes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin,gestionnaire,guichetier")]
    public async Task<IActionResult> Create(CompteCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }

        if (await _compteService.NumeroCompteExisteAsync(vm.NumeroCompte.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.NumeroCompte), "Ce numéro de compte est déjà utilisé.");
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }

        try
        {
            if (!await _compteService.ClientAKycValideAsync(vm.ClientId))
            {
                ModelState.AddModelError(nameof(vm.ClientId),
                    "Impossible d'ouvrir un compte : ce client n'a pas de vérification KYC validée.");
                await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
                return View(vm);
            }

            await _compteService.CreateAsync(vm);
            TempData["Success"] = "Compte ouvert avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError(nameof(vm.NumeroCompte), "Ce numéro de compte est déjà utilisé.");
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }
    }

    // GET: /Comptes/Edit/5
    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> Edit(int id)
    {
        var compte = await _compteService.GetByIdAsync(id);
        if (compte is null) return NotFound();

        var client = await _clientService.GetByIdAsync(compte.ClientId);

        var vm = new CompteEditViewModel
        {
            Id = compte.Id,
            NumeroCompte = compte.NumeroCompte,
            TypeCompteId = compte.TypeCompteId,
            AgenceId = compte.AgenceId,
            Statut = compte.Statut,
            Solde = compte.Solde,
            Devise = compte.Devise,
            ClientNom = client is null ? null : $"{client.Nom} {client.Prenom}"
        };

        await ChargerListesAsync(compte.ClientId, vm.TypeCompteId, vm.AgenceId);
        return View(vm);
    }

    // POST: /Comptes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> Edit(int id, CompteEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(null, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }

        var ok = await _compteService.UpdateAsync(vm);
        if (!ok) return NotFound();

        TempData["Success"] = "Compte modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Comptes/Delete/5
    [Authorize(Roles = "admin,gestionnaire")]
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _compteService.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    // POST: /Comptes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin,gestionnaire")]
    [Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var ok = await _compteService.DeleteAsync(id);
            if (!ok) return NotFound();

            TempData["Success"] = "Compte supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task ChargerListesAsync(
        int? clientId = null, int? typeCompteId = null, int? agenceId = null)
    {
        // Clients
        var clientsVm = await _clientService.GetPagedAsync(null, null, null, 1, int.MaxValue);
        ViewBag.Clients = new SelectList(
            clientsVm.Clients.Select(c => new
            {
                c.Id,
                Libelle = $"{c.NumeroClient} — {c.Nom} {c.Prenom}"
            }),
            "Id", "Libelle", clientId);

        // Agences
        var agencesVm = await _agenceService.GetPagedAsync(null, 1, int.MaxValue);
        ViewBag.Agences = new SelectList(
            agencesVm.Agences.Select(a => new
            {
                a.Id,
                Libelle = $"{a.Code} — {a.Ville}"
            }),
            "Id", "Libelle", agenceId);

        // Types de compte
        var typesVm = await _compteService.GetPagedAsync(null, null, null, null, 1, int.MaxValue);
        ViewBag.TypesCompte = new SelectList(
            typesVm.TypesCompte.Select(t => new
            {
                t.Id,
                Libelle = $"{t.Code} — {t.Libelle}"
            }),
            "Id", "Libelle", typeCompteId);
    }
}
