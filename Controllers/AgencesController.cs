using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

public class AgencesController : Controller
{
    private readonly IAgenceService _agenceService;

    public AgencesController(IAgenceService agenceService)
    {
        _agenceService = agenceService;
    }

    // GET: /Agences
    public async Task<IActionResult> Index(string? recherche, int page = 1)
    {
        const int taille = 10;
        var vm = await _agenceService.GetPagedAsync(recherche, page, taille);
        return View(vm);
    }

    // GET: /Agences/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _agenceService.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    // GET: /Agences/Create
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Create()
    {
        return View(new AgenceCreateViewModel());
    }

    // POST: /Agences/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(AgenceCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Vérification d'unicité du code
        if (await _agenceService.CodeExisteAsync(vm.Code.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.Code), "Ce code d'agence est déjà utilisé.");
            return View(vm);
        }

        try
        {
            await _agenceService.CreateAsync(vm);
            TempData["Success"] = "Agence créée avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sql && sql.Number == 2601)
        {
            // Contrainte UNIQUE violée côté base
            ModelState.AddModelError(nameof(vm.Code), "Ce code d'agence est déjà utilisé.");
            return View(vm);
        }
    }

    // GET: /Agences/Edit/5
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Edit(int id)
    {
        var agence = await _agenceService.GetByIdAsync(id);
        if (agence is null) return NotFound();

        var vm = new AgenceEditViewModel
        {
            Id = agence.Id,
            Code = agence.Code,
            Nom = agence.Nom,
            Ville = agence.Ville,
            EstSiege = agence.EstSiege
        };
        return View(vm);
    }

    // POST: /Agences/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Edit(int id, AgenceEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        if (await _agenceService.CodeExisteAsync(vm.Code.Trim().ToUpperInvariant(), vm.Id))
        {
            ModelState.AddModelError(nameof(vm.Code), "Ce code d'agence est déjà utilisé.");
            return View(vm);
        }

        try
        {
            var ok = await _agenceService.UpdateAsync(vm);
            if (!ok) return NotFound();

            TempData["Success"] = "Agence modifiée avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sql && sql.Number == 2601)
        {
            ModelState.AddModelError(nameof(vm.Code), "Ce code d'agence est déjà utilisé.");
            return View(vm);
        }
    }

    // GET: /Agences/Delete/5
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var agence = await _agenceService.GetByIdAsync(id);
        if (agence is null) return NotFound();
        return View(agence);
    }

    // POST: /Agences/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var ok = await _agenceService.DeleteAsync(id);
            if (!ok) return NotFound();

            TempData["Success"] = "Agence supprimée avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}

