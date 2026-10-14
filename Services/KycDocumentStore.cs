using Microsoft.AspNetCore.DataProtection;

namespace AkarangaBank.Web.Services;

public sealed record StoredKycDocument(string Key, string OriginalName, string ContentType, long Length);

public interface IKycDocumentStore
{
    Task<StoredKycDocument> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<byte[]> ReadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string? key, CancellationToken cancellationToken = default);
}

public sealed class KycDocumentStore : IKycDocumentStore
{
    public const long MaxDocumentBytes = 5 * 1024 * 1024;
    private readonly string _directory;
    private readonly IDataProtector _protector;

    public KycDocumentStore(IWebHostEnvironment environment, IConfiguration configuration, IDataProtectionProvider protectionProvider)
    {
        var configuredPath = configuration["Storage:KycDocumentsPath"];
        _directory = Path.GetFullPath(string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "KycDocuments")
            : Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(environment.ContentRootPath, configuredPath));
        Directory.CreateDirectory(_directory);
        _protector = protectionProvider.CreateProtector("AkarangaBank.KycDocuments.v1");
    }

    public async Task<StoredKycDocument> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Length is <= 0 or > MaxDocumentBytes)
            throw new InvalidDataException("Le document doit peser au maximum 5 Mo.");

        var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => throw new InvalidDataException("Formats autorisés : PDF, PNG et JPEG.")
        };

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await input.CopyToAsync(buffer, cancellationToken);
        var raw = buffer.ToArray();
        if (!HasExpectedSignature(raw, extension))
            throw new InvalidDataException("Le contenu du fichier ne correspond pas à son extension.");

        var key = Guid.NewGuid().ToString("N");
        var protectedBytes = _protector.Protect(raw);
        var path = GetPath(key);
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await output.WriteAsync(protectedBytes, cancellationToken);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }

        var originalName = Path.GetFileName(file.FileName);
        if (originalName.Length > 255) originalName = originalName[^255..];
        return new StoredKycDocument(key, originalName, contentType, file.Length);
    }

    public async Task<byte[]> ReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var encrypted = await File.ReadAllBytesAsync(GetPath(key), cancellationToken);
        return _protector.Unprotect(encrypted);
    }

    public Task DeleteAsync(string? key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key) || !Guid.TryParseExact(key, "N", out _)) return Task.CompletedTask;
        var path = GetPath(key);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string GetPath(string key)
    {
        if (!Guid.TryParseExact(key, "N", out _)) throw new InvalidDataException("Cle de document invalide.");
        var path = Path.GetFullPath(Path.Combine(_directory, key + ".bin"));
        if (!path.StartsWith(_directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Chemin de document invalide.");
        return path;
    }

    private static bool HasExpectedSignature(byte[] bytes, string extension) => extension switch
    {
        ".pdf" => bytes.Length >= 5 && bytes.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
        ".png" => bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        ".jpg" or ".jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
        _ => false
    };
}
