using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin")]
public class ConformiteController : Controller
{
    private readonly IConformiteService _service;
    private readonly AkarangaBankContext _context;
    public ConformiteController(IConformiteService service, AkarangaBankContext context)
    {
        _service = service;
        _context = context;
    }

    public async Task<IActionResult> Seuils(int? agenceId)
    {
        await ChargerAgencesAsync(agenceId);
        return View(new SeuilsAMLViewModel
        {
            AgenceId = agenceId,
            SeuilAML = await _service.GetSeuilAMLAsync(agenceId),
            SeuilDeclarationEspece = await _service.GetSeuilDeclarationEspeceAsync(agenceId),
            SeuilBlocage = await _service.GetSeuilBlocageAsync(agenceId)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> Seuils(SeuilsAMLViewModel vm)
    {
        if (!ModelState.IsValid) { await ChargerAgencesAsync(vm.AgenceId); return View(vm); }
        try
        {
            await _service.EnregistrerSeuilsAsync(vm.SeuilAML, vm.SeuilDeclarationEspece, vm.AgenceId, vm.SeuilBlocage);
            TempData["Success"] = "Les seuils AML et CNRF ont été enregistrés.";
            return RedirectToAction(nameof(Seuils), new { agenceId = vm.AgenceId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerAgencesAsync(vm.AgenceId);
            return View(vm);
        }
    }

    private async Task ChargerAgencesAsync(int? selection)
    {
        var agences = await _context.Agences.AsNoTracking().OrderBy(a => a.Nom).ToListAsync();
        ViewBag.Agences = new SelectList(agences.Select(a => new { a.Id, a.Nom }), "Id", "Nom", selection);
    }
}
