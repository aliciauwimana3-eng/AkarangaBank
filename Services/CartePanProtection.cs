using Microsoft.AspNetCore.DataProtection;

namespace AkarangaBank.Web.Services;

public class CartePanProtection
{
    private readonly IDataProtector _protector;
    public CartePanProtection(IDataProtectionProvider provider) => _protector = provider.CreateProtector("AkarangaBank.Cartes.PAN.v1");
    public string Proteger(string pan) => _protector.Protect(pan);
}
