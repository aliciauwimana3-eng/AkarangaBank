using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using AkarangaBank.Web.Data;
using Hangfire;
using Moq;

namespace AkarangaBank.Web.Tests.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Un seul nom pour toute la vie de la fabrique : sinon chaque requete obtiendrait une base vide differente
    private readonly string _databaseName = $"AkarangaIntegration_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AkarangaBankContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AkarangaBankContext>>();
            services.RemoveAll<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(_ => new EphemeralDataProtectionProvider(NullLoggerFactory.Instance));
            services.AddDbContext<AkarangaBankContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.RemoveAll<IBackgroundJobClient>();
            services.AddSingleton<IBackgroundJobClient>(Mock.Of<IBackgroundJobClient>());    

        });
    }
}
