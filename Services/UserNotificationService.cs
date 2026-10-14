using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using AkarangaBank.Web.Hubs;

namespace AkarangaBank.Web.Services;

public sealed class UserNotificationService : IUserNotificationService
{
    private readonly AkarangaBankContext _context;
    private readonly IHubContext<NotificationHub> _hub;

    public UserNotificationService(AkarangaBankContext context, IHubContext<NotificationHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task NotifyClientAsync(int clientId, string title, string message, string? url = null, CancellationToken cancellationToken = default)
    {
        var recipients = await _context.Users.AsNoTracking()
            .Where(user => user.ClientId == clientId && user.EstActif)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in recipients)
            _context.Set<NotificationUtilisateur>().Add(new NotificationUtilisateur
            {
                UtilisateurId = userId,
                Titre = title,
                Message = message,
                Url = IsLocalUrl(url) ? url : null
            });

        if (recipients.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            await NotifyLiveAsync(recipients, title, message, url, cancellationToken);
        }
    }

    public async Task NotifyKycReviewersAsync(int clientId, string title, string message, string? url = null, CancellationToken cancellationToken = default)
    {
        var agencyId = await _context.Clients.AsNoTracking()
            .Where(client => client.Id == clientId)
            .Select(client => (int?)client.AgenceId)
            .FirstOrDefaultAsync(cancellationToken);
        if (agencyId is null) return;

        var recipients = await _context.Users.AsNoTracking()
            .Where(user => user.EstActif &&
                (user.Role == "admin" || (user.AgenceId == agencyId && (user.Role == "gestionnaire" || user.Role == "guichetier"))))
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in recipients)
            _context.Notifications.Add(new NotificationUtilisateur
            {
                UtilisateurId = userId,
                Titre = title,
                Message = message,
                Url = IsLocalUrl(url) ? url : null
            });

        if (recipients.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            await NotifyLiveAsync(recipients, title, message, url, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<NotificationUtilisateur>> GetForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.Set<NotificationUtilisateur>().AsNoTracking()
            .Where(notification => notification.UtilisateurId == userId)
            .OrderByDescending(notification => notification.DateCreation)
            .Take(100)
            .ToListAsync(cancellationToken);

    public Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default) =>
        _context.Notifications.CountAsync(notification => notification.UtilisateurId == userId && notification.DateLecture == null, cancellationToken);

    public async Task<bool> MarkReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Set<NotificationUtilisateur>()
            .FirstOrDefaultAsync(item => item.Id == notificationId && item.UtilisateurId == userId, cancellationToken);
        if (notification is null) return false;
        notification.DateLecture ??= DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool IsLocalUrl(string? url) => !string.IsNullOrWhiteSpace(url) && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\");

    private async Task NotifyLiveAsync(IEnumerable<int> recipientIds, string title, string message, string? url, CancellationToken cancellationToken)
    {
        foreach (var userId in recipientIds)
            await _hub.Clients.Group($"user-{userId}").SendAsync("NotificationRecue", new { title, message, url }, cancellationToken);
    }
}
