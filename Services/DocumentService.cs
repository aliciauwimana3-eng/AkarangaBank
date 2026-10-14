using System.IO.Compression;
using System.Security.Cryptography;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public sealed class DocumentService : IDocumentService
{
    public const long TailleMaximale = 10 * 1024 * 1024;
    private static readonly HashSet<string> Categories = ["KYC", "Contrat", "Comptable", "AML", "RH", "Audit"];
    private readonly AkarangaBankContext _db;
    private readonly string _directory;
    private readonly IDataProtector _protector;

    public DocumentService(AkarangaBankContext db, IWebHostEnvironment environment, IConfiguration configuration, IDataProtectionProvider provider)
    {
        _db = db;
        var configured = configuration["Storage:DocumentsPath"];
        _directory = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "Documents")
            : Path.IsPathRooted(configured) ? configured : Path.Combine(environment.ContentRootPath, configured));
        Directory.CreateDirectory(_directory);
        _protector = provider.CreateProtector("AkarangaBank.Documents.v1");
    }

    public async Task<Document> DeposerAsync(DocumentDepot depot, CancellationToken cancellationToken = default)
    {
        if (!Categories.Contains(depot.Categorie)) throw new InvalidDataException("Categorie de document invalide.");
        var file = depot.Fichier;
        if (file.Length is <= 0 or > TailleMaximale) throw new InvalidDataException("Le fichier doit peser au maximum 10 Mo.");
        var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
        var bytes = await LireEtValiderAsync(file, extension, cancellationToken);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        var key = Guid.NewGuid().ToString("N");
        var path = GetPath(key);
        try
        {
            await File.WriteAllBytesAsync(path, _protector.Protect(bytes), cancellationToken);
            var doc = new Document
            {
                ClientId = depot.ClientId,
                DeposeParId = depot.UtilisateurId,
                Categorie = depot.Categorie,
                NomOriginal = Path.GetFileName(file.FileName)[..Math.Min(Path.GetFileName(file.FileName).Length, 255)],
                CleStockage = key,
                TypeMime = Mime(extension),
                TailleOctets = bytes.LongLength,
                HashSha256 = hash,
                Version = await _db.Documents.CountAsync(d => d.ClientId == depot.ClientId && d.Categorie == depot.Categorie, cancellationToken) + 1
            };
            _db.Documents.Add(doc);
            _db.DocumentsAcces.Add(new DocumentAcces { Document = doc, UtilisateurId = depot.UtilisateurId, Action = "Depot" });
            await _db.SaveChangesAsync(cancellationToken);
            return doc;
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    public async Task<(Document Document, byte[] Contenu)?> TelechargerAsync(int documentId, int utilisateurId, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.Statut != "Supprime", cancellationToken);
        if (doc is null) return null;
        var bytes = _protector.Unprotect(await File.ReadAllBytesAsync(GetPath(doc.CleStockage), cancellationToken));
        if (!CryptographicOperations.FixedTimeEquals(SHA256.HashData(bytes), Convert.FromHexString(doc.HashSha256)))
            throw new InvalidDataException("Le hash du document ne correspond plus au contenu stocke.");
        _db.DocumentsAcces.Add(new DocumentAcces { DocumentId = doc.Id, UtilisateurId = utilisateurId, Action = "Telechargement" });
        await _db.SaveChangesAsync(cancellationToken);
        return (doc, bytes);
    }

    public async Task<bool> ArchiverAsync(int documentId, int utilisateurId, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.Statut == "Actif", cancellationToken);
        if (doc is null) return false;
        doc.Statut = "Archive";
        doc.DateArchivage = DateTime.UtcNow;
        _db.DocumentsAcces.Add(new DocumentAcces { DocumentId = doc.Id, UtilisateurId = utilisateurId, Action = "Archivage" });
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SupprimerLogiquementAsync(int documentId, int utilisateurId, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.Statut != "Supprime", cancellationToken);
        if (doc is null) return false;
        doc.Statut = "Supprime";
        doc.DateSuppression = DateTime.UtcNow;
        _db.DocumentsAcces.Add(new DocumentAcces { DocumentId = doc.Id, UtilisateurId = utilisateurId, Action = "Suppression" });
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> PurgerAsync(int joursConservation, CancellationToken cancellationToken = default)
    {
        if (joursConservation < 1 || joursConservation > 3650) throw new ArgumentOutOfRangeException(nameof(joursConservation));
        var limite = DateTime.UtcNow.AddDays(-joursConservation);
        var docs = await _db.Documents.Where(d => d.Statut == "Supprime" && d.DateSuppression < limite).ToListAsync(cancellationToken);
        foreach (var doc in docs)
        {
            var path = GetPath(doc.CleStockage);
            if (File.Exists(path)) File.Delete(path);
            _db.Documents.Remove(doc);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return docs.Count;
    }

    private string GetPath(string key)
    {
        if (!Guid.TryParseExact(key, "N", out _)) throw new InvalidDataException("Cle de stockage invalide.");
        var path = Path.GetFullPath(Path.Combine(_directory, key + ".bin"));
        if (!path.StartsWith(_directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Chemin invalide.");
        return path;
    }

    private static async Task<byte[]> LireEtValiderAsync(IFormFile file, string extension, CancellationToken ct)
    {
        if (extension is not (".pdf" or ".jpg" or ".jpeg" or ".png" or ".docx" or ".xlsx")) throw new InvalidDataException("Formats acceptes : PDF, JPG, PNG, DOCX et XLSX.");
        await using var input = file.OpenReadStream();
        using var output = new MemoryStream();
        await input.CopyToAsync(output, ct);
        var bytes = output.ToArray();
        var valid = extension switch
        {
            ".pdf" => bytes.AsSpan().StartsWith("%PDF-"u8),
            ".png" => bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255,
            ".docx" => EstArchiveOfficeValide(bytes, "word/document.xml"),
            ".xlsx" => EstArchiveOfficeValide(bytes, "xl/workbook.xml"),
            _ => false
        };
        if (!valid) throw new InvalidDataException("Le contenu ne correspond pas au format annonce.");
        return bytes;
    }

    private static bool EstArchiveOfficeValide(byte[] bytes, string entry)
    {
        try
        {
            using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            return archive.GetEntry(entry) is not null;
        }
        catch (InvalidDataException) { return false; }
    }

    private static string Mime(string ext) => ext switch
    {
        ".pdf" => "application/pdf", ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _ => "application/octet-stream"
    };
}
