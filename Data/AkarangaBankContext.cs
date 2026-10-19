using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Data;

public class AkarangaBankContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public AkarangaBankContext(DbContextOptions<AkarangaBankContext> options)
        : base(options)
    {
    }

    // ---------- DbSet métier ----------
    public DbSet<Agence> Agences => Set<Agence>();
    public DbSet<Parametre> Parametres => Set<Parametre>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<VerificationKYC> VerificationsKYC => Set<VerificationKYC>();
    public DbSet<TypeCompte> TypesCompte => Set<TypeCompte>();
    public DbSet<Compte> Comptes => Set<Compte>();
    public DbSet<Carte> Cartes => Set<Carte>();
    public DbSet<Pret> Prets => Set<Pret>();
    public DbSet<TypePret> TypesPret => Set<TypePret>();
    public DbSet<EcheancePret> EcheancesPret => Set<EcheancePret>();
    public DbSet<TransactionCarte> TransactionsCartes => Set<TransactionCarte>();
    public DbSet<Virement> Virements => Set<Virement>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Ecriture> Ecritures => Set<Ecriture>();
    public DbSet<CompteComptable> ComptesComptables => Set<CompteComptable>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AlerteAML> AlertesAML => Set<AlerteAML>();
    public DbSet<SessionUtilisateur> SessionsUtilisateur => Set<SessionUtilisateur>();
    public DbSet<NotificationUtilisateur> Notifications => Set<NotificationUtilisateur>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentAcces> DocumentsAcces => Set<DocumentAcces>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    // Alias français vers Users (défini par IdentityDbContext)
    public DbSet<ApplicationUser> Utilisateurs { get => Users; set => Users = value; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // IMPORTANT : Identity d'abord

        // ============ 1. IDENTITY : renommage des tables ============
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Utilisateurs");
            b.Property(u => u.NomComplet).HasMaxLength(100).IsRequired();
            b.Property(u => u.Role).HasMaxLength(20).IsRequired();
            b.Property(u => u.DeuxFA_Secret).HasMaxLength(255);
            b.Property(u => u.PushToken).HasMaxLength(512);

            b.HasOne(u => u.Agence).WithMany()
                .HasForeignKey(u => u.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(u => u.Client).WithMany()
                .HasForeignKey(u => u.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(u => u.AgenceId);
            b.HasIndex(u => u.Role);
        });

        modelBuilder.Entity<ApplicationRole>(b => b.ToTable("Roles"));
        modelBuilder.Entity<IdentityUserRole<int>>(b => b.ToTable("UtilisateursRoles"));
        modelBuilder.Entity<IdentityUserClaim<int>>(b => b.ToTable("UtilisateursClaims"));
        modelBuilder.Entity<IdentityUserLogin<int>>(b => b.ToTable("UtilisateursLogins"));
        modelBuilder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("RolesClaims"));
        modelBuilder.Entity<IdentityUserToken<int>>(b => b.ToTable("UtilisateursTokens"));

        // ============ 2. AGENCE ============
        modelBuilder.Entity<Agence>(b =>
        {
            b.ToTable("Agences");
            b.Property(a => a.Code).HasMaxLength(20).IsRequired();
            b.Property(a => a.Nom).HasMaxLength(100).IsRequired();
            b.Property(a => a.Ville).HasMaxLength(50).IsRequired();
            b.Property(a => a.DateCreation).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(a => a.Code).IsUnique();
        });

        // ============ 3. PARAMETRE ============
        modelBuilder.Entity<Parametre>(b =>
        {
            b.ToTable("Parametres");
            b.Property(p => p.Cle).HasMaxLength(50).IsRequired();
            b.Property(p => p.Valeur).HasMaxLength(255).IsRequired();
            b.Property(p => p.Description).HasMaxLength(255);
            b.Property(p => p.DateModification).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(p => new { p.Cle, p.AgenceId }).IsUnique();

            b.HasOne(p => p.Agence).WithMany()
                .HasForeignKey(p => p.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 4. CLIENT ============
        modelBuilder.Entity<Client>(b =>
        {
            b.ToTable("Clients");
            b.Property(c => c.NumeroClient).HasMaxLength(20).IsRequired();
            b.Property(c => c.Nom).HasMaxLength(100).IsRequired();
            b.Property(c => c.Prenom).HasMaxLength(100).IsRequired();
            b.Property(c => c.DateNaissance).HasColumnType("date");
            b.Property(c => c.NumeroCNI).HasMaxLength(30).IsRequired();
            b.Property(c => c.Telephone).HasMaxLength(20);
            b.Property(c => c.Email).HasMaxLength(100);
            b.Property(c => c.Adresse).HasMaxLength(255);
            b.Property(c => c.Statut).HasMaxLength(20).HasDefaultValue("Actif");
            b.Property(c => c.DateCreation).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(c => c.NumeroClient).IsUnique();
            b.HasIndex(c => c.NumeroCNI).IsUnique();
            b.HasIndex(c => c.AgenceId);

            b.HasOne(c => c.Agence).WithMany()
                .HasForeignKey(c => c.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 5. VERIFICATION KYC ============
        modelBuilder.Entity<VerificationKYC>(b =>
        {
            b.ToTable("VerificationsKYC");
            b.Property(k => k.TypeDocument).HasMaxLength(50).IsRequired();
            b.Property(k => k.NumeroDocument).HasMaxLength(50).IsRequired();
            b.Property(k => k.CheminDocument).HasMaxLength(500);
            b.Property(k => k.Statut).HasMaxLength(20).HasDefaultValue("EnAttente");
            b.Property(k => k.Commentaire).HasMaxLength(500);

            b.HasOne(k => k.Client).WithMany(c => c.VerificationsKYC)
                .HasForeignKey(k => k.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(k => k.VerifiePar).WithMany()
                .HasForeignKey(k => k.VerifieParId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 6. TYPE DE COMPTE ============
        modelBuilder.Entity<TypeCompte>(b =>
        {
            b.ToTable("TypesCompte");
            b.Property(t => t.Code).HasMaxLength(20).IsRequired();
            b.Property(t => t.Libelle).HasMaxLength(50).IsRequired();
            b.Property(t => t.TauxInteret).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
            b.Property(t => t.SoldeMinimum).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            b.Property(t => t.DateCreation).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(t => t.Code).IsUnique();
        });

        // ============ 7. COMPTE ============
        modelBuilder.Entity<Compte>(b =>
        {
            b.ToTable("Comptes");
            b.Property(c => c.NumeroCompte).HasMaxLength(30).IsRequired();
            b.Property(c => c.Solde).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            b.Property(c => c.Devise).HasMaxLength(3).HasDefaultValue("BIF");
            b.Property(c => c.Statut).HasMaxLength(20).HasDefaultValue("Actif");
            b.Property(c => c.DateOuverture).HasDefaultValueSql("timezone('utc', now())");
            b.Property(c => c.DateCreation).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(c => c.NumeroCompte).IsUnique();
            b.HasIndex(c => c.ClientId);
            b.HasIndex(c => c.AgenceId);
            b.HasIndex(c => c.TypeCompteId);

            b.HasOne(c => c.Client).WithMany(cl => cl.Comptes)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(c => c.TypeCompte).WithMany()
                .HasForeignKey(c => c.TypeCompteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(c => c.Agence).WithMany()
                .HasForeignKey(c => c.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 8. CARTE ============
        modelBuilder.Entity<Carte>(b =>
        {
            b.ToTable("Cartes");
            b.Property(c => c.NumeroCarte).HasMaxLength(20).IsRequired();
            b.Property(c => c.NumeroCarteChiffre).HasMaxLength(500).IsRequired();
            b.Property(c => c.DerniersChiffres).HasMaxLength(4).IsRequired();
            b.Property(c => c.TypeCarte).HasMaxLength(20).IsRequired();
            b.Property(c => c.Statut).HasMaxLength(20).HasDefaultValue("Active");
            b.Property(c => c.MotifBlocage).HasMaxLength(20);
            b.Property(c => c.PlafondRetraitJournalier).HasColumnType("decimal(18,2)");
            b.Property(c => c.PlafondPaiementJournalier).HasColumnType("decimal(18,2)");
            b.Property(c => c.PlafondEnLigneJournalier).HasColumnType("decimal(18,2)");
            b.Property(c => c.DateCreation).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(c => c.NumeroCarte).IsUnique();
            b.HasIndex(c => c.CompteId);
            b.HasIndex(c => c.ClientId);

            b.HasOne(c => c.Compte).WithMany(co => co.Cartes)
                .HasForeignKey(c => c.CompteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(c => c.Client).WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransactionCarte>(b =>
        {
            b.ToTable("TransactionsCartes");
            b.Property(t => t.Montant).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(t => t.TypeTransaction).HasMaxLength(20).IsRequired();
            b.Property(t => t.Statut).HasMaxLength(20).IsRequired();
            b.Property(t => t.MotifRefus).HasMaxLength(250);
            b.HasIndex(t => new { t.CarteId, t.DateTransaction });
            b.HasOne(t => t.Carte).WithMany(c => c.Transactions).HasForeignKey(t => t.CarteId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(t => t.Operation).WithMany().HasForeignKey(t => t.OperationId).OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 9. PRET ============
        modelBuilder.Entity<TypePret>(b =>
        {
            b.ToTable("TypesPret");
            b.Property(t => t.Code).HasMaxLength(30).IsRequired();
            b.Property(t => t.Libelle).HasMaxLength(100).IsRequired();
            b.Property(t => t.MontantMin).HasColumnType("decimal(18,2)");
            b.Property(t => t.MontantMax).HasColumnType("decimal(18,2)");
            b.Property(t => t.TauxMin).HasColumnType("decimal(5,2)");
            b.Property(t => t.TauxMax).HasColumnType("decimal(5,2)");
            b.HasIndex(t => t.Code).IsUnique();
            b.HasData(
                new TypePret { Id = 1, Code = "PERSONNEL", Libelle = "Prêt personnel", MontantMin = 100000m, MontantMax = 10000000m, DureeMinMois = 3, DureeMaxMois = 60, TauxMin = 8m, TauxMax = 16m },
                new TypePret { Id = 2, Code = "EQUIPEMENT", Libelle = "Prêt équipement", MontantMin = 500000m, MontantMax = 50000000m, DureeMinMois = 6, DureeMaxMois = 120, TauxMin = 6m, TauxMax = 14m });
        });

        modelBuilder.Entity<Pret>(b =>
        {
            b.ToTable("Prets");
            b.Property(p => p.Reference).HasMaxLength(30).IsRequired();
            b.Property(p => p.MontantPrincipal).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(p => p.TauxInteret).HasColumnType("decimal(5,2)").IsRequired();
            b.Property(p => p.Mensualite).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(p => p.MontantRembourse).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            b.Property(p => p.MotifRejet).HasMaxLength(500);
            b.Property(p => p.Commentaire).HasMaxLength(500);
            b.Property(p => p.Statut).HasMaxLength(20).HasDefaultValue("EnAttente");
            b.Property(p => p.DateDemande).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(p => p.Reference).IsUnique();
            b.HasIndex(p => p.ClientId);
            b.HasIndex(p => p.Statut);

            b.HasOne(p => p.Client).WithMany()
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.TypePret).WithMany()
                .HasForeignKey(p => p.TypePretId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.CompteDebit).WithMany()
                .HasForeignKey(p => p.CompteDebitId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.ValidePar).WithMany()
                .HasForeignKey(p => p.ValideParId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 10. ECHEANCE PRET ============
        modelBuilder.Entity<EcheancePret>(b =>
        {
            b.ToTable("EcheancesPret");
            b.Property(e => e.Capital).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(e => e.Interet).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(e => e.MontantTotal).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(e => e.Penalite).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            b.Property(e => e.Statut).HasMaxLength(20).HasDefaultValue("EnAttente");

            b.HasIndex(e => new { e.PretId, e.NumeroEcheance }).IsUnique();
            b.HasIndex(e => e.DateEcheance);

            b.HasOne(e => e.Pret).WithMany(p => p.Echeances)
                .HasForeignKey(e => e.PretId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============ 11. VIREMENT ============
        modelBuilder.Entity<Virement>(b =>
        {
            b.ToTable("Virements");
            b.Property(v => v.Reference).HasMaxLength(30).IsRequired();
            b.Property(v => v.Montant).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(v => v.Devise).HasMaxLength(3).HasDefaultValue("BIF");
            b.Property(v => v.Statut).HasMaxLength(20).HasDefaultValue("EnAttente");
            b.Property(v => v.Motif).HasMaxLength(255);
            b.Property(v => v.ControleAML).HasDefaultValue(false);
            b.Property(v => v.EstInterbancaire).HasDefaultValue(false);
            b.Property(v => v.BanqueDestination).HasMaxLength(100);
            b.Property(v => v.NumeroCompteExterne).HasMaxLength(34);
            b.Property(v => v.BeneficiaireExterne).HasMaxLength(150);
            b.Property(v => v.DateVirement).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(v => v.Reference).IsUnique();
            b.HasIndex(v => v.CompteSourceId);
            b.HasIndex(v => v.CompteDestinationId);
            b.HasIndex(v => v.Statut);

            b.HasOne(v => v.CompteSource).WithMany()
                .HasForeignKey(v => v.CompteSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(v => v.CompteDestination).WithMany()
                .HasForeignKey(v => v.CompteDestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(v => v.Utilisateur).WithMany()
                .HasForeignKey(v => v.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 12. OPERATION ============
        modelBuilder.Entity<Operation>(b =>
        {
            b.ToTable("Operations");
            b.Property(o => o.Reference).HasMaxLength(30).IsRequired();
            b.Property(o => o.TypeOperation).HasMaxLength(30).IsRequired();
            b.Property(o => o.Montant).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(o => o.Devise).HasMaxLength(3).HasDefaultValue("BIF");
            b.Property(o => o.Statut).HasMaxLength(20).HasDefaultValue("EnAttente");
            b.Property(o => o.Commentaire).HasMaxLength(255);
            b.Property(o => o.DateOperation).HasDefaultValueSql("timezone('utc', now())");
            b.Property(o => o.DateCreation).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(o => o.Reference).IsUnique();
            b.HasIndex(o => o.CompteId);
            b.HasIndex(o => o.UtilisateurId);
            b.HasIndex(o => o.AgenceId);
            b.HasIndex(o => o.DateOperation);
            b.HasIndex(o => o.Statut);

            b.HasOne(o => o.Compte).WithMany()
                .HasForeignKey(o => o.CompteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(o => o.Utilisateur).WithMany()
                .HasForeignKey(o => o.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(o => o.Agence).WithMany()
                .HasForeignKey(o => o.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(o => o.Virement).WithMany()
                .HasForeignKey(o => o.VirementId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(o => o.Pret).WithMany()
                .HasForeignKey(o => o.PretId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 13. ECRITURE (partie double) ============
        modelBuilder.Entity<Ecriture>(b =>
        {
            b.ToTable("Ecritures", table => table.HasCheckConstraint(
                "CK_Ecritures_UnSeulCompte",
                "(CompteId IS NULL AND CompteComptableId IS NOT NULL) OR (CompteId IS NOT NULL AND CompteComptableId IS NULL)"));
            b.Property(e => e.Sens).HasMaxLength(10).IsRequired();
            b.Property(e => e.Montant).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(e => e.Devise).HasMaxLength(3).HasDefaultValue("BIF");
            b.Property(e => e.DateEcriture).HasDefaultValueSql("timezone('utc', now())");
            b.Property(e => e.Libelle).HasMaxLength(255);

            b.HasIndex(e => e.OperationId);
            b.HasIndex(e => e.CompteId);

            b.HasOne(e => e.Operation).WithMany(o => o.Ecritures)
                .HasForeignKey(e => e.OperationId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(e => e.Compte).WithMany(c => c.Ecritures)
                .HasForeignKey(e => e.CompteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(e => e.CompteComptable).WithMany(c => c.Ecritures)
                .HasForeignKey(e => e.CompteComptableId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(e => e.Agence).WithMany()
                .HasForeignKey(e => e.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => e.AgenceId);
        });

        modelBuilder.Entity<CompteComptable>(b =>
        {
            b.ToTable("ComptesComptables");
            b.Property(c => c.Code).HasMaxLength(30).IsRequired();
            b.Property(c => c.Libelle).HasMaxLength(100).IsRequired();
            b.Property(c => c.Devise).HasMaxLength(3).IsRequired();
            b.HasIndex(c => c.Code).IsUnique();
        });

        // ============ 14. AUDIT LOG ============
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLog");   // table au singulier, comme dans le script
            b.Property(a => a.DateEvenement).HasDefaultValueSql("timezone('utc', now())");
            b.Property(a => a.Action).HasMaxLength(50).IsRequired();
            b.Property(a => a.TableConcernee).HasMaxLength(50);
            b.Property(a => a.ValeurAvant).HasColumnType("text");
            b.Property(a => a.ValeurApres).HasColumnType("text");
            b.Property(a => a.AdresseIP).HasMaxLength(45);
            b.Property(a => a.UserAgent).HasMaxLength(255);
            b.Property(a => a.Details).HasColumnType("text");

            b.HasIndex(a => a.DateEvenement);
            b.HasIndex(a => a.UtilisateurId);

            b.HasOne(a => a.Utilisateur).WithMany()
                .HasForeignKey(a => a.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(a => a.Agence).WithMany()
                .HasForeignKey(a => a.AgenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 15. ALERTE AML ============
        modelBuilder.Entity<AlerteAML>(b =>
        {
            b.ToTable("AlertesAML");
            b.Property(a => a.TypeAlerte).HasMaxLength(50).IsRequired();
            b.Property(a => a.Niveau).HasMaxLength(20).IsRequired();
            b.Property(a => a.Description).HasMaxLength(500);
            b.Property(a => a.Statut).HasMaxLength(20).HasDefaultValue("Ouverte");
            b.Property(a => a.DateAlerte).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(a => a.Statut);
            b.HasIndex(a => a.ClientId);

            b.HasOne(a => a.Client).WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(a => a.Operation).WithMany(o => o.Alertes)
                .HasForeignKey(a => a.OperationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(a => a.TraitePar).WithMany()
                .HasForeignKey(a => a.TraiteParId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ 16. SESSION UTILISATEUR ============
        modelBuilder.Entity<SessionUtilisateur>(b =>
        {
            b.ToTable("SessionsUtilisateur");
            b.Property(s => s.Token).HasMaxLength(255).IsRequired();
            b.Property(s => s.AdresseIP).HasMaxLength(45);
            b.Property(s => s.UserAgent).HasMaxLength(255);
            b.Property(s => s.DateConnexion).HasDefaultValueSql("timezone('utc', now())");

            b.HasIndex(s => s.Token).IsUnique();
            b.HasIndex(s => s.UtilisateurId);

            b.HasOne(s => s.Utilisateur).WithMany()
                .HasForeignKey(s => s.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationUtilisateur>(b =>
        {
            b.ToTable("NotificationsUtilisateurs");
            b.Property(n => n.Titre).HasMaxLength(120).IsRequired();
            b.Property(n => n.Message).HasMaxLength(1000).IsRequired();
            b.Property(n => n.Url).HasMaxLength(500);
            b.Property(n => n.Canal).HasMaxLength(20).HasDefaultValue("Interne");
            b.Property(n => n.Statut).HasMaxLength(30).HasDefaultValue("Envoyee");
            b.Property(n => n.TemplateCode).HasMaxLength(80);
            b.Property(n => n.Langue).HasMaxLength(10).HasDefaultValue("fr-BI");
            b.Property(n => n.TemplateVersion);
            b.Property(n => n.Erreur).HasMaxLength(1000);
            b.Property(n => n.DateCreation).HasDefaultValueSql("timezone('utc', now())");
            b.HasIndex(n => new { n.UtilisateurId, n.DateLecture, n.DateCreation });
            b.HasOne(n => n.Utilisateur).WithMany()
                .HasForeignKey(n => n.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationTemplate>(b =>
        {
            b.ToTable("NotificationTemplates");
            b.Property(t => t.Code).HasMaxLength(80).IsRequired();
            b.Property(t => t.Canal).HasMaxLength(20).IsRequired();
            b.Property(t => t.Langue).HasMaxLength(10).IsRequired();
            b.Property(t => t.Sujet).HasMaxLength(255).IsRequired();
            b.Property(t => t.ContenuTexte).HasMaxLength(10000).IsRequired();
            b.Property(t => t.ContenuHtml).HasMaxLength(20000);
            b.HasIndex(t => new { t.Code, t.Canal, t.Langue, t.Version }).IsUnique();
        });

        modelBuilder.Entity<Document>(b =>
        {
            b.ToTable("Documents");
            b.Property(d => d.Categorie).HasMaxLength(20).IsRequired();
            b.Property(d => d.NomOriginal).HasMaxLength(255).IsRequired();
            b.Property(d => d.CleStockage).HasMaxLength(32).IsRequired();
            b.Property(d => d.TypeMime).HasMaxLength(150).IsRequired();
            b.Property(d => d.HashSha256).HasMaxLength(64).IsRequired();
            b.Property(d => d.Statut).HasMaxLength(20).HasDefaultValue("Actif");
            b.HasIndex(d => new { d.ClientId, d.Categorie, d.Version }).IsUnique();
            b.HasIndex(d => new { d.Statut, d.DateSuppression });
            b.HasOne(d => d.Client).WithMany().HasForeignKey(d => d.ClientId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(d => d.DeposePar).WithMany().HasForeignKey(d => d.DeposeParId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentAcces>(b =>
        {
            b.ToTable("DocumentsAcces");
            b.Property(a => a.Action).HasMaxLength(30).IsRequired();
            b.HasIndex(a => new { a.DocumentId, a.DateAcces });
            b.HasOne(a => a.Document).WithMany(d => d.Acces).HasForeignKey(a => a.DocumentId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.Utilisateur).WithMany().HasForeignKey(a => a.UtilisateurId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VerificationKYC>(b =>
        {
            b.Property(k => k.NomDocumentOriginal).HasMaxLength(255);
            b.Property(k => k.TypeMimeDocument).HasMaxLength(100);
        });
    }
}

