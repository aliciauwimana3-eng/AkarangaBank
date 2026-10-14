using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public class ComptabiliteService : IComptabiliteService
{
    private readonly AkarangaBankContext _context;
    public ComptabiliteService(AkarangaBankContext context) => _context = context;

    public async Task<JournalViewModel> GetJournalAsync(DateTime? du, DateTime? au, string? typeOperation,
        int page, int taille, int utilisateurId)
    {
        page = Math.Max(1, page);
        taille = Math.Clamp(taille, 1, 100);
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null) return new JournalViewModel { Page = page, TaillePage = taille };
        var query = _context.Operations.AsNoTracking().AsQueryable();
        if (user.Role == "client")
        {
            if (!user.ClientId.HasValue) return new JournalViewModel { Page = page, TaillePage = taille };
            query = query.Where(o => o.Compte.ClientId == user.ClientId.Value);
        }
        else if (user.Role != "admin") query = query.Where(o => o.AgenceId == user.AgenceId ||
            o.Ecritures.Any(e => e.AgenceId == user.AgenceId));
        if (du.HasValue) query = query.Where(o => o.DateOperation >= du.Value);
        if (au.HasValue) query = query.Where(o => o.DateOperation < au.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(typeOperation)) query = query.Where(o => o.TypeOperation == typeOperation);
        var total = await query.CountAsync();
        var operations = await query.Include(o => o.Compte).Include(o => o.Utilisateur).Include(o => o.Ecritures)
            .OrderByDescending(o => o.DateOperation).Skip((page - 1) * taille).Take(taille).ToListAsync();
        return new JournalViewModel { Operations = operations, Du = du, Au = au, TypeOperation = typeOperation, Page = page, TaillePage = taille, Total = total };
    }

    public async Task<ReleveViewModel?> GetReleveAsync(int compteId, int utilisateurId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == utilisateurId && u.EstActif);
        if (user is null) return null;
        var query = _context.Comptes.AsNoTracking().Where(c => c.Id == compteId);
        if (user.Role == "client")
        {
            if (!user.ClientId.HasValue) return null;
            query = query.Where(c => c.ClientId == user.ClientId.Value);
        }
        else if (user.Role != "admin") query = query.Where(c => c.AgenceId == user.AgenceId);
        var compte = await query.Include(c => c.Client).FirstOrDefaultAsync();
        if (compte is null) return null;
        var ecritures = await _context.Ecritures.AsNoTracking().Include(e => e.Operation)
            .Where(e => e.CompteId == compteId).OrderBy(e => e.DateEcriture).ThenBy(e => e.Id).ToListAsync();
        var credits = ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
        var debits = ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant);
        var soldeInitial = compte.Solde - credits + debits;
        return new ReleveViewModel { Compte = compte, SoldeInitial = soldeInitial, SoldeFinal = soldeInitial + credits - debits, Ecritures = ecritures };
    }

    public async Task<decimal> GetSoldeAsync(int compteId, int utilisateurId)
    {
        var releve = await GetReleveAsync(compteId, utilisateurId);
        return releve?.SoldeFinal ?? throw new InvalidOperationException("Compte introuvable ou accès non autorisé.");
    }

    public async Task<bool> VerifierEquilibreGlobalAsync()
    {
        var differences = await _context.Ecritures.AsNoTracking().GroupBy(e => e.OperationId)
            .Select(g => g.Sum(e => e.Sens == "Debit" ? e.Montant : -e.Montant))
            .Where(diff => diff != 0).AnyAsync();
        return !differences;
    }
}
