using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,gestionnaire,auditeur")]
public class AlertesAMLController : Controller
{
    private readonly IDeclarationService _service;
    public AlertesAMLController(IDeclarationService service) => _service = service;

    public async Task<IActionResult> Index(string? statut, string? niveau)
    {
        var id = UtilisateurId();
        return View(new AlerteAMLListViewModel
        {
            Alertes = await _service.GetAlertesAsync(statut, niveau, id),
            Statut = statut,
            Niveau = niveau
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExporterDeclarationsCNRF()
    {
        var declarations = await _service.GetDeclarationsCNRFAsync(UtilisateurId());
        static string Csv(string? value)
        {
            var safe = value?.Trim() ?? string.Empty;
            if (safe.StartsWith('=') || safe.StartsWith('+') || safe.StartsWith('-') || safe.StartsWith('@')) safe = "'" + safe;
            return $"\"{safe.Replace("\"", "\"\"")}\"";
        }
        var lignes = new List<string>
        {
            "ReferenceAlerte;Date;NumeroClient;Client;Operation;Montant;Devise;Statut;Description"
        };
        lignes.AddRange(declarations.Select(a => string.Join(';', new[]
        {
            Csv(a.Id.ToString()),
            Csv(a.DateAlerte.ToString("yyyy-MM-dd HH:mm:ss")),
            Csv(a.Client.NumeroClient),
            Csv($"{a.Client.Nom} {a.Client.Prenom}"),
            Csv(a.Operation?.Reference),
            Csv(a.Operation?.Montant.ToString("0.00")),
            Csv(a.Operation?.Devise),
            Csv(a.Statut),
            Csv(a.Description)
        })));
        var contenu = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(string.Join("\r\n", lignes))).ToArray();
        return File(contenu, "text/csv; charset=utf-8", $"declarations-cnrf-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "admin,gestionnaire"), Authorize(Policy = "RequireMfa")]
    public async Task<IActionResult> Traiter(int id, string statut, string? commentaire)
    {
        try
        {
            if (!await _service.TraiterAlerteAsync(id, UtilisateurId(), statut, commentaire)) return NotFound();
            TempData["Success"] = "Le traitement de l'alerte a été enregistré.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private int UtilisateurId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new InvalidOperationException("Impossible d'identifier l'utilisateur connecté.");
}
