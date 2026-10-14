using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;
[Authorize(Roles = "admin,gestionnaire,guichetier")]
public class VerificationsKYCController : Controller
{
    private readonly IVerificationKYCService _kycService;
    private readonly IClientService _clientService;
    private readonly IKycDocumentStore _documentStore;
    private readonly IUserNotificationService _notifications;

    public VerificationsKYCController(
        IVerificationKYCService kycService,
        IClientService clientService,
        IKycDocumentStore documentStore,
        IUserNotificationService notifications)
    {
        _kycService = kycService;
        _clientService = clientService;
        _documentStore = documentStore;
        _notifications = notifications;
    }

    // GET: /VerificationsKYC/Create?clientId=5
    public async Task<IActionResult> Create(int clientId)
    {
        var client = await _clientService.GetByIdAsync(clientId);
        if (client is null) return NotFound();

        return View(new VerificationKYCCreateViewModel
        {
            ClientId = client.Id,
            ClientNom = $"{client.Nom} {client.Prenom}",
            ClientNumero = client.NumeroClient
        });
    }

    // POST: /VerificationsKYC/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(KycDocumentStore.MaxDocumentBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = KycDocumentStore.MaxDocumentBytes + 1024 * 1024)]
    public async Task<IActionResult> Create(VerificationKYCCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        StoredKycDocument stored;
        try
        {
            stored = await _documentStore.SaveAsync(vm.Fichier!, HttpContext.RequestAborted);
        }
        catch (InvalidDataException ex)
        {
            ModelState.AddModelError(nameof(vm.Fichier), ex.Message);
            return View(vm);
        }

        vm.DocumentKey = stored.Key;
        vm.NomDocumentOriginal = stored.OriginalName;
        vm.TypeMimeDocument = stored.ContentType;
        vm.TailleDocument = stored.Length;

        // L'utilisateur vérificateur est fictif pour l'instant (id=2 : gestionnaire).
        // L'identité réelle sera branchée au chapitre 7.
        const int verifieParId = 2;

        try
        {
            await _kycService.CreateAsync(vm, verifieParId);
        }
        catch
        {
            await _documentStore.DeleteAsync(stored.Key, HttpContext.RequestAborted);
            throw;
        }
        await _notifications.NotifyClientAsync(vm.ClientId, "Document KYC reçu", "Votre justificatif a été reçu et attend sa vérification.", $"/Clients/Details/{vm.ClientId}", HttpContext.RequestAborted);
        await _notifications.NotifyKycReviewersAsync(vm.ClientId, "Nouveau justificatif KYC", "Un justificatif attend votre vérification.", $"/Clients/Details/{vm.ClientId}", HttpContext.RequestAborted);
        TempData["Success"] = "Vérification KYC créée. En attente de validation.";
        return RedirectToAction("Details", "Clients", new { id = vm.ClientId });
    }

    [HttpGet]
    public async Task<IActionResult> Document(int id, CancellationToken cancellationToken)
    {
        var verification = await _kycService.GetByIdAsync(id);
        if (verification is null || string.IsNullOrWhiteSpace(verification.CheminDocument)) return NotFound();

        try
        {
            var content = await _documentStore.ReadAsync(verification.CheminDocument, cancellationToken);
            var contentType = verification.TypeMimeDocument is "application/pdf" or "image/png" or "image/jpeg"
                ? verification.TypeMimeDocument
                : "application/octet-stream";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(content, contentType, verification.NomDocumentOriginal ?? "document-kyc");
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return Problem("Le document ne peut pas être déchiffré.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // POST: /VerificationsKYC/Valider/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Valider(int id)
    {
        const int verifieParId = 2;
        var ok = await _kycService.ValiderAsync(id, verifieParId);
        if (!ok) return NotFound();

        TempData["Success"] = "Vérification KYC validée.";
        var verification = await _kycService.GetByIdAsync(id);
        if (verification is not null)
            await _notifications.NotifyClientAsync(verification.ClientId, "Justificatif KYC validé", "Votre justificatif a été validé.", $"/Clients/Details/{verification.ClientId}", HttpContext.RequestAborted);
        return RedirectToAction("Index", "Clients");
    }

    // POST: /VerificationsKYC/Rejeter/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rejeter(int id, string commentaire)
    {
        const int verifieParId = 2;
        var motif = (commentaire ?? "").Trim();
        if (motif.Length > 500) motif = motif[..500];
        var ok = await _kycService.RejeterAsync(id, verifieParId, motif);
        if (!ok) return NotFound();

        TempData["Success"] = "Vérification KYC rejetée.";
        var verification = await _kycService.GetByIdAsync(id);
        if (verification is not null)
            await _notifications.NotifyClientAsync(verification.ClientId, "Justificatif KYC rejeté", motif, $"/Clients/Details/{verification.ClientId}", HttpContext.RequestAborted);
        return RedirectToAction("Index", "Clients");
    }
}
