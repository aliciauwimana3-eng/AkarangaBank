using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Http;

namespace AkarangaBank.Web.Services;

public sealed record DocumentDepot(int ClientId, int UtilisateurId, string Categorie, IFormFile Fichier);

public interface IDocumentService
{
    Task<Document> DeposerAsync(DocumentDepot depot, CancellationToken cancellationToken = default);
    Task<(Document Document, byte[] Contenu)?> TelechargerAsync(int documentId, int utilisateurId, CancellationToken cancellationToken = default);
    Task<bool> ArchiverAsync(int documentId, int utilisateurId, CancellationToken cancellationToken = default);
    Task<bool> SupprimerLogiquementAsync(int documentId, int utilisateurId, CancellationToken cancellationToken = default);
    Task<int> PurgerAsync(int joursConservation, CancellationToken cancellationToken = default);
}
