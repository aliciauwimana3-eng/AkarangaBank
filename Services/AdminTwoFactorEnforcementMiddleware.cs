using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace AkarangaBank.Web.Services;

/// <summary>
/// Oblige les administrateurs connectés à activer l'authentification à deux facteurs
/// avant d'utiliser le reste de l'application.
/// </summary>
public class AdminTwoFactorEnforcementMiddleware
{
    private const string SetupPath = "/Identity/Account/Manage/EnableAuthenticator";

    private static readonly string[] AllowedPrefixes =
    {
        "/Identity/Account/Manage",
        "/Identity/Account/Logout",
        "/Identity/Account/Login",
        "/health",
        "/hubs"
    };

    private readonly RequestDelegate _next;

    public AdminTwoFactorEnforcementMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.User.IsInRole("admin")
            && !AllowedPrefixes.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is not null && !await userManager.GetTwoFactorEnabledAsync(user))
            {
                context.Response.Redirect(SetupPath);
                return;
            }
        }

        await _next(context);
    }
}
