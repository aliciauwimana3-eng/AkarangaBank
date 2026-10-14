namespace AkarangaBank.Web.Services;

public interface ISmsService
{
    Task SendAsync(string destination, string message, CancellationToken cancellationToken = default);
}
