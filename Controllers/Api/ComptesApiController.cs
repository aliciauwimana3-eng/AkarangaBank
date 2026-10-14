using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/v1/comptes")]
public class ComptesApiController : ControllerBase
{
    private readonly IOperationService _operations;
    private readonly IComptabiliteService _comptabilite;

    public ComptesApiController(IOperationService operations, IComptabiliteService comptabilite)
    {
        _operations = operations;
        _comptabilite = comptabilite;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompteApiDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompteApiDto>>> GetComptes()
    {
        var id = UtilisateurId();
        var comptes = await _operations.GetComptesActifsAsync(id);
        return Ok(comptes.Select(c => new CompteApiDto(
            c.Id, c.NumeroCompte, c.Client.Nom, c.Client.Prenom, c.Solde, c.Devise, c.Statut)).ToList());
    }

    [HttpGet("{id:int}/solde")]
    [ProducesResponseType(typeof(SoldeApiDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoldeApiDto>> GetSolde(int id)
    {
        try
        {
            var solde = await _comptabilite.GetSoldeAsync(id, UtilisateurId());
            return Ok(new SoldeApiDto(id, solde));
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Compte introuvable." });
        }
    }

    [HttpGet("{id:int}/releve")]
    [ProducesResponseType(typeof(ReleveApiDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReleveApiDto>> GetReleve(int id)
    {
        var releve = await _comptabilite.GetReleveAsync(id, UtilisateurId());
        if (releve is null) return NotFound(new { message = "Compte introuvable." });
        return Ok(new ReleveApiDto(
            releve.Compte.Id,
            releve.Compte.NumeroCompte,
            releve.Compte.Devise,
            releve.SoldeInitial,
            releve.SoldeFinal,
            releve.Ecritures.Select(e => new EcritureApiDto(
                e.DateEcriture,
                e.Operation.Reference,
                e.Libelle,
                e.Sens,
                e.Montant,
                e.Devise)).ToList()));
    }

    private int UtilisateurId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new UnauthorizedAccessException("Utilisateur non authentifié.");
}

public sealed record CompteApiDto(int Id, string NumeroCompte, string Nom, string Prenom, decimal Solde, string Devise, string Statut);
public sealed record SoldeApiDto(int CompteId, decimal Solde);
public sealed record EcritureApiDto(DateTime Date, string Reference, string? Libelle, string Sens, decimal Montant, string Devise);
public sealed record ReleveApiDto(int CompteId, string NumeroCompte, string Devise, decimal SoldeInitial, decimal SoldeFinal, IReadOnlyList<EcritureApiDto> Ecritures);
