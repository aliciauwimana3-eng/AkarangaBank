namespace AkarangaBank.Web.Services;

public interface IEmailService
{
    Task SendAsync(string destination, string subject, string text, string? html, CancellationToken cancellationToken = default);
}
