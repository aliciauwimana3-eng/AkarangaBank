namespace AkarangaBank.Web.Services;

public interface IPushNotificationService
{
    Task SendAsync(string token, string title, string message, CancellationToken cancellationToken = default);
}
