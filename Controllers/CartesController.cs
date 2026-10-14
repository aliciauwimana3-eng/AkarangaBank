using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Controllers;

public class CartesController : Controller
{
    private readonly AkarangaBankContext _db;
    private readonly ICarteService _service;
    private readonly CartePanProtection _panProtection;
    public CartesController(AkarangaBankContext db, ICarteService service, CartePanProtection panProtection) { _db = db; _service = service; _panProtection = panProtection; }

    public async Task<IActionResult> Index()
    {
        var user = await UtilisateurAsync();
        if (user is null) return Forbid();
        var query = _db.Cartes.AsNoTracking().Include(c => c.Client).Include(c => c.Compte).AsQueryable();
        if (user.Role == "client")
        {
            if (!user.ClientId.HasValue) return Forbid();
            query = query.Where(c => c.ClientId == user.ClientId.Value);
        }
        else if (user.Role != "admin") query = query.Where(c => c.Compte.AgenceId == user.AgenceId);
        return View(await query.OrderByDescending(c => c.DateCreation).Take(200).ToListAsync());
    }

    [Authorize(Policy = "GestionnaireOrAdmin")]
    public async Task<IActionResult> Create()
    {
        await ChargerComptesAsync();
        return View(new CarteCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "GestionnaireOrAdmin"), Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> Create(CarteCreateViewModel vm)
    {
        var user = await UtilisateurAsync();
        var compte = await _db.Comptes.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == vm.CompteId);
        if (user is null || compte is null || user.Role != "admin" && compte.AgenceId != user.AgenceId)
            ModelState.AddModelError(nameof(vm.CompteId), "Compte introuvable ou hors de votre agence.");
        else if (compte.Statut != "Actif" || compte.Client.Statut != "Actif" ||
                 !await _db.VerificationsKYC.AnyAsync(k => k.ClientId == compte.ClientId && k.Statut == "Valide"))
            ModelState.AddModelError(nameof(vm.CompteId), "Le compte et le client doivent être actifs et le KYC validé.");
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }

        var pan = CarteService.GenererNumero(vm.TypeCarte);
        var numero = $"**** **** **** {pan[^4..]}";
        while (await _db.Cartes.AnyAsync(c => c.NumeroCarte == numero))
        {
            pan = CarteService.GenererNumero(vm.TypeCarte);
            numero = $"**** **** **** {pan[^4..]}";
        }
        _db.Cartes.Add(new Carte
        {
            NumeroCarte = numero, NumeroCarteChiffre = _panProtection.Proteger(pan), DerniersChiffres = pan[^4..], CompteId = compte!.Id, ClientId = compte.ClientId,
            TypeCarte = vm.TypeCarte, DateExpiration = DateTime.UtcNow.Date.AddYears(3),
            Statut = "Active", DateCreation = DateTime.UtcNow,
            PlafondRetraitJournalier = vm.PlafondRetraitJournalier,
            PlafondPaiementJournalier = vm.PlafondPaiementJournalier,
            PlafondEnLigneJournalier = vm.PlafondEnLigneJournalier
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Carte {numero} créée. Seuls les quatre derniers chiffres sont conservés.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Transaction(int id, decimal montant, string typeTransaction)
    {
        try
        {
            var result = await _service.AutoriserTransactionAsync(id, UtilisateurId(), montant, typeTransaction);
            TempData[result.Statut == "Autorisee" ? "Success" : "Error"] = result.Statut == "Autorisee"
                ? "Transaction autorisée et comptabilisée." : $"Transaction refusée : {result.MotifRefus}";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (UnauthorizedAccessException) { return Forbid(); }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Historique(int id) => View(await _service.GetHistoriqueAsync(id, UtilisateurId()));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Bloquer(int id, string motif)
    {
        try { await _service.BloquerAsync(id, UtilisateurId(), motif); TempData["Success"] = "La carte a été mise en opposition."; }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return NotFound(ex.Message); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Renouveler(int id)
    {
        try { await _service.RenouvelerAsync(id, UtilisateurId()); TempData["Success"] = "La carte a été renouvelée."; }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private Task<ApplicationUser?> UtilisateurAsync()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id && u.EstActif)
            : Task.FromResult<ApplicationUser?>(null);
    }

    private int UtilisateurId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new InvalidOperationException("Utilisateur non authentifié.");

    private async Task ChargerComptesAsync(int? selected = null)
    {
        var user = await UtilisateurAsync();
        var query = _db.Comptes.AsNoTracking().Where(c => c.Statut == "Actif");
        if (user?.Role != "admin") query = query.Where(c => c.AgenceId == user!.AgenceId);
        var comptes = await query.OrderBy(c => c.NumeroCompte).ToListAsync();
        ViewBag.Comptes = new SelectList(comptes.Select(c => new { c.Id, Libelle = $"{c.NumeroCompte} ({c.Devise})" }), "Id", "Libelle", selected);
    }
}
