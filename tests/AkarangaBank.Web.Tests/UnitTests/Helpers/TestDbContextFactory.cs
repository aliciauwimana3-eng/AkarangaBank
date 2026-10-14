using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AkarangaBank.Web.Data;

namespace AkarangaBank.Web.Tests.UnitTests.Helpers;

public static class TestDbContextFactory
{
    public static AkarangaBankContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AkarangaBankContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new AkarangaBankContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
