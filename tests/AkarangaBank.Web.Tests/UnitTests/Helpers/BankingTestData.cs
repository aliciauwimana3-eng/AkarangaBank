using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Tests.UnitTests.Helpers;

public static class BankingTestData
{
    public static async Task<(Agence Agence, Client Client, Compte Compte, ApplicationUser Utilisateur)> SeedAccountAsync(
        AkarangaBankContext db, decimal solde = 20_000_000m)
    {
        var agence = new Agence { Id = 1, Code = "TEST", Nom = "Agence test", Ville = "Gitega", EstSiege = true };
        var client = new Client
        {
            Id = 1, NumeroClient = "CL-TEST", Nom = "Test", Prenom = "Client",
            DateNaissance = new DateTime(1990, 1, 1), NumeroCNI = "CNI-TEST", Agence = agence, AgenceId = agence.Id
        };
        var typeCompte = new TypeCompte { Id = 1, Code = "COURANT", Libelle = "Compte courant" };
        var compte = new Compte
        {
            Id = 1, NumeroCompte = "CP-TEST", Client = client, ClientId = client.Id,
            TypeCompte = typeCompte, TypeCompteId = typeCompte.Id,
            Agence = agence, AgenceId = agence.Id, Devise = "BIF", Solde = solde, Statut = "Actif"
        };
        var utilisateur = new ApplicationUser
        {
            Id = 1, UserName = "test@akaranga.local", NormalizedUserName = "TEST@AKARANGA.LOCAL",
            Email = "test@akaranga.local", NormalizedEmail = "TEST@AKARANGA.LOCAL",
            NomComplet = "Utilisateur test", Role = "admin", Agence = agence, AgenceId = agence.Id, EstActif = true
        };
        db.AddRange(agence, client, typeCompte, compte, utilisateur);
        await db.SaveChangesAsync();
        return (agence, client, compte, utilisateur);
    }

    public static Operation CreateOperation(Compte compte, ApplicationUser utilisateur, Agence agence, string reference = "OP-TEST") => new()
    {
        Reference = reference, TypeOperation = "Depot", Compte = compte, CompteId = compte.Id,
        Utilisateur = utilisateur, UtilisateurId = utilisateur.Id, Agence = agence, AgenceId = agence.Id,
        Montant = 10_000m, Devise = "BIF", Statut = "Valide", DateOperation = DateTime.UtcNow
    };
}
