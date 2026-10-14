using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public static class AdminInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var context = services.GetRequiredService<AkarangaBankContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminInitializer");

        // Une agence siège initiale rend possible le premier compte administrateur sur une base neuve.
        if (!await context.Agences.AnyAsync())
        {
            context.Agences.Add(new Agence
            {
                Code = "SIEGE", Nom = "Akaranga Bank — Siège", Ville = "Bujumbura",
                EstSiege = true, DateCreation = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // 1. Créer les rôles manquants
        string[] roles = { "admin", "gestionnaire", "auditeur", "guichetier", "client" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        if (!await context.Parametres.AnyAsync(p => p.Cle == "SEUIL_AML" && p.AgenceId == null))
        {
            context.Parametres.Add(new Parametre
            {
                Cle = "SEUIL_AML",
                Valeur = "5000000",
                Description = "Seuil de vigilance anti-blanchiment (BIF)",
                DateModification = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // 2. Créer le premier administrateur (adresse et mot de passe dans les User Secrets)
        var adminEmail = config["AdminInitial:Email"];
        var adminPassword = config["AdminInitial:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("AdminInitial:Email ou AdminInitial:Password manquant : aucun administrateur créé.");
            return;
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is not null) return;

        var siege = await context.Agences.FirstOrDefaultAsync(a => a.EstSiege);
        if (siege is null)
        {
            logger.LogWarning("Aucune agence siège trouvée : administrateur non créé.");
            return;
        }

        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            NomComplet = "Administrateur Système",
            Role = "admin",
            AgenceId = siege.Id,
            EstActif = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "admin");
        else
            logger.LogError("Création de l'administrateur impossible : {Erreurs}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
