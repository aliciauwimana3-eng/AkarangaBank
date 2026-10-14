using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Tests.UnitTests.Services;

public class AuditSaveChangesInterceptorTests
{
    [Fact]
    public async Task EntreeAudit_RefuseLaModificationApresCreation()
    {
        var options = new DbContextOptionsBuilder<AkarangaBankContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .AddInterceptors(new AuditSaveChangesInterceptor(new HttpContextAccessor()))
            .Options;
        await using var db = new AkarangaBankContext(options);
        var audit = new AuditLog { Action = "Creation_Test", TableConcernee = "Tests" };
        db.AuditLogs.Add(audit);
        await db.SaveChangesAsync();

        audit.Details = "Modification interdite";
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
}
