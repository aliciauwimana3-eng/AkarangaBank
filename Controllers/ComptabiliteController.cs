using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Controllers;

public class ComptabiliteController : Controller
{
    private readonly IComptabiliteService _service;
    public ComptabiliteController(IComptabiliteService service) => _service = service;
    [Authorize(Roles = "admin,gestionnaire,auditeur")]

    public async Task<IActionResult> Journal(DateTime? du, DateTime? au, string? typeOperation, int page = 1)
    {
        var vm = await _service.GetJournalAsync(du, au, typeOperation, page, 20, UtilisateurId());
        return View(vm);
    }

    public async Task<IActionResult> Releve(int compteId)
    {
        var vm = await _service.GetReleveAsync(compteId, UtilisateurId());
        return vm is null ? NotFound() : View(vm);
    }

    [Authorize(Roles = "admin,auditeur")]
    public async Task<IActionResult> VerifierEquilibre()
    {
        ViewBag.EstEquilibre = await _service.VerifierEquilibreGlobalAsync();
        return View();
    }

    private int UtilisateurId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new InvalidOperationException("Impossible d'identifier l'utilisateur connecté.");
}
