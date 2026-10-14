using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public class VerificationKYCService : IVerificationKYCService
{
    private readonly AkarangaBankContext _context;

    public VerificationKYCService(AkarangaBankContext context)
    {
        _context = context;
    }

    public async Task<VerificationKYC> CreateAsync(
        VerificationKYCCreateViewModel vm, int? verifieParId)
    {
        var kyc = new VerificationKYC
        {
            ClientId = vm.ClientId,
            TypeDocument = vm.TypeDocument.Trim(),
            NumeroDocument = vm.NumeroDocument.Trim().ToUpperInvariant(),
            CheminDocument = vm.DocumentKey,
            NomDocumentOriginal = vm.NomDocumentOriginal,
            TypeMimeDocument = vm.TypeMimeDocument,
            TailleDocument = vm.TailleDocument,
            Commentaire = vm.Commentaire?.Trim(),
            Statut = "EnAttente"
        };

        _context.VerificationsKYC.Add(kyc);
        await _context.SaveChangesAsync();
        return kyc;
    }

    public async Task<bool> ValiderAsync(int id, int verifieParId)
    {
        var kyc = await _context.VerificationsKYC.FindAsync(id);
        if (kyc is null) return false;

        kyc.Statut = "Valide";
        kyc.VerifieParId = verifieParId;
        kyc.DateVerification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejeterAsync(int id, int verifieParId, string commentaire)
    {
        var kyc = await _context.VerificationsKYC.FindAsync(id);
        if (kyc is null) return false;

        kyc.Statut = "Rejete";
        kyc.VerifieParId = verifieParId;
        kyc.DateVerification = DateTime.UtcNow;
        kyc.Commentaire = commentaire;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<VerificationKYC?> GetByIdAsync(int id)
    {
        return await _context.VerificationsKYC
            .Include(v => v.Client)
            .Include(v => v.VerifiePar)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}
