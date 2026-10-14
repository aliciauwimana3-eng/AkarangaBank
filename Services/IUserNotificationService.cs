using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface IUserNotificationService
{
    Task NotifyClientAsync(int clientId, string title, string message, string? url = null, CancellationToken cancellationToken = default);
    Task NotifyKycReviewersAsync(int clientId, string title, string message, string? url = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationUtilisateur>> GetForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> MarkReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default);
}
