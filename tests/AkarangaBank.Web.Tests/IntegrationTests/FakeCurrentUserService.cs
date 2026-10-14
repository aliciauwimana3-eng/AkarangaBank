using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Tests.IntegrationTests;

// Remplace CurrentUserService : l'utilisateur factice des tests n'existe pas en base
public class FakeCurrentUserService : ICurrentUserService
{
    public Task<int> GetUserIdAsync() => Task.FromResult(1);
    public Task<int> GetAgenceIdAsync() => Task.FromResult(1);
}
