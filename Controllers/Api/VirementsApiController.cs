using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/v1/virements")]
public class VirementsApiController : ControllerBase
{
    private readonly IVirementService _service;
    public VirementsApiController(IVirementService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VirementApiDto>>> GetVirements()
    {
        var virements = await _service.GetRecentAsync(UtilisateurId());
        return Ok(virements.Select(v => new VirementApiDto(
            v.Id,
            v.Reference,
            v.CompteSource.NumeroCompte,
            v.EstInterbancaire ? v.BanqueDestination : v.CompteDestination?.NumeroCompte,
            v.BeneficiaireExterne,
            v.EstInterbancaire,
            v.Montant,
            v.Devise,
            v.Statut,
            v.DateVirement)).ToList());
    }

    [HttpPost]
    [ProducesResponseType(typeof(VirementCreeApiDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VirementCreeApiDto>> CreerVirement([FromBody] VirementCreateViewModel demande)
    {
        try
        {
            var virement = await _service.CreateAsync(demande, UtilisateurId());
            var resultat = new VirementCreeApiDto(virement.Id, virement.Reference, virement.Statut, virement.Montant, virement.Devise);
            return Accepted(resultat);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int UtilisateurId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new UnauthorizedAccessException("Utilisateur non authentifié.");
}

public sealed record VirementApiDto(int Id, string Reference, string CompteSource, string? Destinataire, string? Beneficiaire,
    bool EstInterbancaire, decimal Montant, string Devise, string Statut, DateTime DateCreation);
public sealed record VirementCreeApiDto(int Id, string Reference, string Statut, decimal Montant, string Devise);
