using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

public sealed class DocumentsController : Controller
{
    private readonly AkarangaBankContext _db;
    private readonly IDocumentService _documents;
    private readonly ICurrentUserService _currentUser;

    public DocumentsController(AkarangaBankContext db, IDocumentService documents, ICurrentUserService currentUser)
    {
        _db = db;
        _documents = documents;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = await _currentUser.GetUserIdAsync();
        var query = _db.Documents.AsNoTracking().Where(d => d.Statut != "Supprime");
        if (User.IsInRole("client"))
        {
            var clientId = await _db.Users.Where(u => u.Id == userId).Select(u => u.ClientId).FirstOrDefaultAsync(cancellationToken);
            query = query.Where(d => d.ClientId == clientId);
        }
        else if (!User.IsInRole("admin"))
        {
            var agencyId = await _db.Users.Where(u => u.Id == userId).Select(u => u.AgenceId).FirstAsync(cancellationToken);
            query = query.Where(d => d.Client.AgenceId == agencyId);
        }
        return View(await query.OrderByDescending(d => d.DateCreation).Take(300).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (document is null) return NotFound();
        var userId = await _currentUser.GetUserIdAsync();
        if (!await CanAccessAsync(document.ClientId, userId, cancellationToken)) return Forbid();
        IReadOnlyList<DocumentAcces> accesses = User.IsInRole("client") ? [] : await _db.DocumentsAcces.AsNoTracking().Include(a => a.Utilisateur)
            .Where(a => a.DocumentId == id).OrderByDescending(a => a.DateAcces).Take(100).ToListAsync(cancellationToken);
        return View(new DocumentDetailsViewModel { Document = document, Acces = accesses });
    }

    [Authorize(Roles = "admin,gestionnaire,guichetier")]
    public IActionResult Upload(int clientId) => View(new DocumentUploadViewModel { ClientId = clientId });

    [HttpPost, Authorize(Roles = "admin,gestionnaire,guichetier"), ValidateAntiForgeryToken]
    [RequestSizeLimit(IDocumentServiceFileLimit.Value)]
    [RequestFormLimits(MultipartBodyLengthLimit = IDocumentServiceFileLimit.Value)]
    public async Task<IActionResult> Upload(DocumentUploadViewModel model, CancellationToken cancellationToken)
    {
        var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == model.ClientId, cancellationToken);
        var userId = await _currentUser.GetUserIdAsync();
        if (client is null) return NotFound();
        if (!User.IsInRole("admin"))
        {
            var agencyId = await _db.Users.Where(u => u.Id == userId).Select(u => u.AgenceId).FirstAsync(cancellationToken);
            if (agencyId != client.AgenceId) return Forbid();
        }
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _documents.DeposerAsync(new DocumentDepot(model.ClientId, userId, model.Categorie, model.Fichier!), cancellationToken);
            TempData["Success"] = "Document depose.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidDataException ex)
        {
            ModelState.AddModelError(nameof(model.Fichier), ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Telecharger(int id, CancellationToken cancellationToken)
    {
        var userId = await _currentUser.GetUserIdAsync();
        var document = await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id && d.Statut != "Supprime", cancellationToken);
        if (document is null) return NotFound();
        if (!await CanAccessAsync(document.ClientId, userId, cancellationToken)) return Forbid();
        try
        {
            var result = await _documents.TelechargerAsync(id, userId, cancellationToken);
            if (result is null) return NotFound();
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(result.Value.Contenu, result.Value.Document.TypeMime, result.Value.Document.NomOriginal);
        }
        catch (FileNotFoundException) { return NotFound(); }
        catch (Exception exception) when (exception is System.Security.Cryptography.CryptographicException or InvalidDataException)
        {
            return Problem("Le document stocke ne peut pas etre valide.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost, Authorize(Roles = "admin,gestionnaire"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Archiver(int id, CancellationToken cancellationToken) =>
        await ChangeStatusAsync(id, archive: true, cancellationToken);

    [HttpPost, Authorize(Roles = "admin,gestionnaire"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(int id, CancellationToken cancellationToken) =>
        await ChangeStatusAsync(id, archive: false, cancellationToken);

    private async Task<IActionResult> ChangeStatusAsync(int id, bool archive, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (document is null) return NotFound();
        var userId = await _currentUser.GetUserIdAsync();
        if (!User.IsInRole("admin") && !await CanAccessAsync(document.ClientId, userId, cancellationToken)) return Forbid();
        var updated = archive
            ? await _documents.ArchiverAsync(id, userId, cancellationToken)
            : await _documents.SupprimerLogiquementAsync(id, userId, cancellationToken);
        if (!updated) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanAccessAsync(int clientId, int userId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("admin")) return true;
        if (User.IsInRole("client"))
            return await _db.Users.AnyAsync(u => u.Id == userId && u.ClientId == clientId, cancellationToken);
        var agencyId = await _db.Users.Where(u => u.Id == userId).Select(u => (int?)u.AgenceId).FirstOrDefaultAsync(cancellationToken);
        return agencyId.HasValue && await _db.Clients.AnyAsync(c => c.Id == clientId && c.AgenceId == agencyId, cancellationToken);
    }

    private static class IDocumentServiceFileLimit
    {
        public const long Value = 11 * 1024 * 1024;
    }
}
