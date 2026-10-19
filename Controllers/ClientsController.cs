using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;
[Authorize(Roles = "admin,gestionnaire,guichetier")] 

public class ClientsController : Controller
{
    private readonly IClientService _clientService;
    private readonly IAgenceService _agenceService;

    public ClientsController(IClientService clientService, IAgenceService agenceService)
    {
        _clientService = clientService;
        _agenceService = agenceService;
    }

    // GET: /Clients
    public async Task<IActionResult> Index(
        string? recherche, int? agenceId, string? statut, int page = 1)
    {
        const int taille = 10;
        var vm = await _clientService.GetPagedAsync(recherche, agenceId, statut, page, taille);
        return View(vm);
    }

    // GET: /Clients/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _clientService.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    // GET: /Clients/Create
    public async Task<IActionResult> Create()
    {
        await ChargerAgencesAsync();
        return View(new ClientCreateViewModel());
    }

    // POST: /Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        if (await _clientService.NumeroClientExisteAsync(vm.NumeroClient.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.NumeroClient), "Ce numéro de client est déjà utilisé.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        if (await _clientService.NumeroCNIExisteAsync(vm.NumeroCNI.Trim().ToUpperInvariant()))
        {
            ModelState.AddModelError(nameof(vm.NumeroCNI), "Ce numéro de CNI est déjà utilisé.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        try
        {
            await _clientService.CreateAsync(vm);
            TempData["Success"] = "Client créé avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("", "Un client avec ce numéro ou ce CNI existe déjà.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }
    }

    // GET: /Clients/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client is null) return NotFound();

        var vm = new ClientEditViewModel
        {
            Id = client.Id,
            NumeroClient = client.NumeroClient,
            Nom = client.Nom,
            Prenom = client.Prenom,
            DateNaissance = client.DateNaissance,
            NumeroCNI = client.NumeroCNI,
            Telephone = client.Telephone,
            Email = client.Email,
            Adresse = client.Adresse,
            AgenceId = client.AgenceId,
            Statut = client.Statut
        };

        await ChargerAgencesAsync(vm.AgenceId);
        return View(vm);
    }

    // POST: /Clients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        if (await _clientService.NumeroClientExisteAsync(vm.NumeroClient.Trim().ToUpperInvariant(), vm.Id))
        {
            ModelState.AddModelError(nameof(vm.NumeroClient), "Ce numéro de client est déjà utilisé.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        if (await _clientService.NumeroCNIExisteAsync(vm.NumeroCNI.Trim().ToUpperInvariant(), vm.Id))
        {
            ModelState.AddModelError(nameof(vm.NumeroCNI), "Ce numéro de CNI est déjà utilisé.");
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }

        var ok = await _clientService.UpdateAsync(vm);
        if (!ok) return NotFound();

        TempData["Success"] = "Client modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clients/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _clientService.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    // POST: /Clients/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var ok = await _clientService.DeleteAsync(id);
            if (!ok) return NotFound();

            TempData["Success"] = "Client supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Clients/ExportCsv?recherche=...&agenceId=...&statut=...
    public async Task<IActionResult> ExportCsv(string? recherche, int? agenceId, string? statut)
    {
        // Tous les clients correspondant aux filtres (sans pagination)
        var vm = await _clientService.GetPagedAsync(recherche, agenceId, statut, 1, int.MaxValue);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Numero;Nom;Prenom;CNI;Telephone;Email;Agence;Statut");

        foreach (var c in vm.Clients)
        {
            sb.AppendLine(string.Join(";",
                Csv(c.NumeroClient), Csv(c.Nom), Csv(c.Prenom), Csv(c.NumeroCNI),
                Csv(c.Telephone), Csv(c.Email), Csv(c.Agence.Code), Csv(c.Statut)));
        }

        // BOM UTF-8 pour qu'Excel affiche correctement les accents
        var contenu = System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();

        return File(contenu, "text/csv", $"clients_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    // Entoure la valeur de guillemets et double les guillemets internes
    private static string Csv(string? valeur)
    {
        return "\"" + (valeur ?? "").Replace("\"", "\"\"") + "\"";
    }

    // Remplit la liste déroulante des agences (ViewBag.Agences)
    private async Task ChargerAgencesAsync(int? selectedId = null)
    {
        var agencesVm = await _agenceService.GetPagedAsync(null, 1, int.MaxValue);
        ViewBag.Agences = new SelectList(
            agencesVm.Agences.Select(a => new
            {
                a.Id,
                Libelle = $"{a.Code} — {a.Nom}"
            }),
            "Id", "Libelle", selectedId);
    }
}



