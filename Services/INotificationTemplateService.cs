namespace AkarangaBank.Web.Services;

public sealed record NotificationRendered(string Subject, string? Html, string Text, int Version);

public interface INotificationTemplateService
{
    Task<NotificationRendered?> RenderAsync(string code, string canal, string langue, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken = default);
}
