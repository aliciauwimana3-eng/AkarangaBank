using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.Net;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using AkarangaBank.Web.Hubs;
using Hangfire;
using System.Threading.RateLimiting;
using Hangfire.PostgreSql;

// PostgreSQL : accepte les dates sans fuseau horaire, comme SQL Server
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
    {
        if (IPAddress.TryParse(proxy, out var address)) options.KnownProxies.Add(address);
    }
     // Hebergeur avec proxy inverse (Railway) : on fait confiance au proxy de la plateforme
    if (builder.Configuration.GetValue<bool>("ForwardedHeaders:TrustAll"))
    {
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
});

// --- Base de données (chaîne de connexion dans les User Secrets) ---
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<AkarangaBankContext>((services, options) =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("AkarangaBank"))
        .AddInterceptors(services.GetRequiredService<AuditSaveChangesInterceptor>()));


// --- Identity ---
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Politique de mot de passe bancaire
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Verrouillage après tentatives échouées
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Confirmation d'e-mail désactivée pour le cours
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;

    // Jeton TOTP pour la 2FA
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    options.Tokens.AuthenticatorIssuer = "AkarangaBank";
})
.AddEntityFrameworkStores<AkarangaBankContext>()
.AddDefaultTokenProviders();

// --- Cookie d'authentification ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    // DUREE_SESSION_MIN = 30 minutes dans le script ; marge de 60 minutes glissantes
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// --- Services métier ---
builder.Services.AddScoped<IAgenceService, AgenceService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IVerificationKYCService, VerificationKYCService>();
builder.Services.AddScoped<IKycDocumentStore, KycDocumentStore>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<NotificationBackgroundJob>();
builder.Services.AddScoped<DocumentPurgeHangfireJob>();
builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();
builder.Services.AddScoped<ICompteService, CompteService>();
builder.Services.AddScoped<IVirementService, VirementService>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IPretService, PretService>();
builder.Services.AddScoped<ICarteService, CarteService>();
builder.Services.AddSingleton<CartePanProtection>();
// Les noms de ressources compilés sont AkarangaBank.Web.SharedResource[.culture].resources.
builder.Services.AddLocalization(options => options.ResourcesPath = string.Empty);
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .SetApplicationName("AkarangaBank")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
builder.Services.AddScoped<LocalizationService>();
var supportedCultures = LocalizationService.SupportedCultures.ToList();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("fr-BI");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddScoped<IConformiteService, ConformiteService>();
builder.Services.AddScoped<ISurveillanceService, SurveillanceService>();
builder.Services.AddScoped<IDeclarationService, DeclarationService>();
builder.Services.AddScoped<IComptabiliteService, ComptabiliteService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddSingleton<ReportingExportService>();
builder.Services.AddSingleton<ReportingPdfService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();

// --- Politiques d'autorisation ---
builder.Services.AddSingleton<IAuthorizationHandler, RequireMfaHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireMfa", policy =>
        policy.Requirements.Add(new RequireMfa()));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("GuichetierOnly", policy =>
        policy.RequireRole("guichetier", "gestionnaire", "admin"));

    options.AddPolicy("GestionnaireOrAdmin", policy =>
        policy.RequireRole("gestionnaire", "admin"));
});

// --- Protection globale : tout exige l'authentification ---
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress ?? IPAddress.None,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddSignalR();
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("AkarangaBank"))));
    builder.Services.AddHangfireServer();
}

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/LoginWith2fa");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/LoginWithRecoveryCode");
});

var app = builder.Build();

app.UseForwardedHeaders();

// --- Initialisation : rôles + administrateur ---
using (var scope = app.Services.CreateScope())
{
    if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        var database = scope.ServiceProvider.GetRequiredService<AkarangaBankContext>();
        await database.Database.MigrateAsync();
    }
    await AdminInitializer.InitializeAsync(scope.ServiceProvider);
    if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        var database = scope.ServiceProvider.GetRequiredService<AkarangaBankContext>();
        await NotificationTemplateSeeder.SeedAsync(database);
    }
}

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();   // AVANT UseAuthorization
app.UseAuthorization();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthorizationFilter()]
    });
    RecurringJob.AddOrUpdate<DocumentPurgeHangfireJob>("documents-purge", job => job.RunAsync(), Cron.Daily);
    RecurringJob.AddOrUpdate<NotificationBackgroundJob>("loan-due-reminders", job => job.PlanifierRappelsEcheancesAsync(), Cron.Daily);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapGet("/health/live", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapGet("/health/ready", async (AkarangaBankContext context) =>
{
    try
    {
        return await context.Database.CanConnectAsync()
            ? Results.Ok(new { status = "ready" })
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();

app.Run();

public partial class Program { }