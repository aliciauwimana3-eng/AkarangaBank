namespace AkarangaBank.Web.Services;

public static class ReferenceGenerator
{
    // Format : PREFIXE-AAAAMMJJ-XXXXXX (6 caracteres hexadecimaux en majuscules)
    public static string Generer(string prefix)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var alea = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"{prefix}-{date}-{alea}";
    }
}
