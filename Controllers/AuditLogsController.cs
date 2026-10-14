using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Controllers;

[Authorize(Roles = "admin,auditeur")]
public class AuditLogsController : Controller
{
    private readonly AkarangaBankContext _context;
    private readonly ICurrentUserService _currentUser;
    public AuditLogsController(AkarangaBankContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? table, string? action, int page = 1)
    {
        page = Math.Max(1, page);
        const int pageSize = 50;
        var userId = await _currentUser.GetUserIdAsync();
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.EstActif);
        if (user is null) return Forbid();

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();
        if (user.Role != "admin") query = query.Where(a => a.AgenceId == user.AgenceId);
        if (!string.IsNullOrWhiteSpace(table)) query = query.Where(a => a.TableConcernee == table);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(a => a.Action == action);
        var total = await query.CountAsync();
        var logs = await query.Include(a => a.Utilisateur).Include(a => a.Agence)
            .OrderByDescending(a => a.DateEvenement).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.Table = table;
        ViewBag.Action = action;
        return View(logs);
    }
}
