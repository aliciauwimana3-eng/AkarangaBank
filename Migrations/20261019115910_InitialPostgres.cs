using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ville = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstSiege = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateModification = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComptesComptables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Devise = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComptesComptables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Langue = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Sujet = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContenuHtml = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    ContenuTexte = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypesCompte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TauxInteret = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    SoldeMinimum = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    AutoriseDecouvert = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesCompte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypesPret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MontantMin = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantMax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DureeMinMois = table.Column<int>(type: "integer", nullable: false),
                    DureeMaxMois = table.Column<int>(type: "integer", nullable: false),
                    TauxMin = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TauxMax = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesPret", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroClient = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "date", nullable: false),
                    NumeroCNI = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Adresse = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AgenceId = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Actif"),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateModification = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parametres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Valeur = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AgenceId = table.Column<int>(type: "integer", nullable: true),
                    DateModification = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parametres_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolesClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comptes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCompte = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    TypeCompteId = table.Column<int>(type: "integer", nullable: false),
                    AgenceId = table.Column<int>(type: "integer", nullable: false),
                    Solde = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Devise = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BIF"),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Actif"),
                    DateOuverture = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateFermeture = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateModification = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comptes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comptes_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comptes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comptes_TypesCompte_TypeCompteId",
                        column: x => x.TypeCompteId,
                        principalTable: "TypesCompte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomComplet = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AgenceId = table.Column<int>(type: "integer", nullable: false),
                    DeuxFA_Active = table.Column<bool>(type: "boolean", nullable: false),
                    DeuxFA_Secret = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PushToken = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DerniereConnexion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateModification = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cartes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCarte = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompteId = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    TypeCarte = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NumeroCarteChiffre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DerniersChiffres = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    PlafondRetraitJournalier = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlafondPaiementJournalier = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlafondEnLigneJournalier = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    MotifBlocage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cartes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cartes_Comptes_CompteId",
                        column: x => x.CompteId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateEvenement = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: true),
                    AgenceId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TableConcernee = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EnregistrementId = table.Column<int>(type: "integer", nullable: true),
                    ValeurAvant = table.Column<string>(type: "text", nullable: true),
                    ValeurApres = table.Column<string>(type: "text", nullable: true),
                    AdresseIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLog_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    DeposeParId = table.Column<int>(type: "integer", nullable: true),
                    Categorie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NomOriginal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CleStockage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TypeMime = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TailleOctets = table.Column<long>(type: "bigint", nullable: false),
                    HashSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Actif"),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateArchivage = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateSuppression = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Utilisateurs_DeposeParId",
                        column: x => x.DeposeParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationsUtilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    Titre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateLecture = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Interne"),
                    Statut = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Envoyee"),
                    TemplateCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Langue = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "fr-BI"),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: true),
                    Tentatives = table.Column<int>(type: "integer", nullable: false),
                    Erreur = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DateEnvoi = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationsUtilisateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationsUtilisateurs_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reference = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TypePretId = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    CompteDebitId = table.Column<int>(type: "integer", nullable: false),
                    ValideParId = table.Column<int>(type: "integer", nullable: true),
                    MontantPrincipal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TauxInteret = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    DureeMois = table.Column<int>(type: "integer", nullable: false),
                    Mensualite = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente"),
                    DateDemande = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateValidation = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MontantRembourse = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    MotifRejet = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Commentaire = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prets_Comptes_CompteDebitId",
                        column: x => x.CompteDebitId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prets_TypesPret_TypePretId",
                        column: x => x.TypePretId,
                        principalTable: "TypesPret",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prets_Utilisateurs_ValideParId",
                        column: x => x.ValideParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionsUtilisateur",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AdresseIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DeuxFA_Valide = table.Column<bool>(type: "boolean", nullable: false),
                    DateConnexion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateExpiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionsUtilisateur", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionsUtilisateur_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UtilisateursClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UtilisateursClaims_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UtilisateursLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UtilisateursLogins_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UtilisateursRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UtilisateursRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UtilisateursRoles_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UtilisateursTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilisateursTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UtilisateursTokens_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationsKYC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    TypeDocument = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumeroDocument = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CheminDocument = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NomDocumentOriginal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TypeMimeDocument = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TailleDocument = table.Column<long>(type: "bigint", nullable: true),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente"),
                    VerifieParId = table.Column<int>(type: "integer", nullable: true),
                    DateVerification = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Commentaire = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationsKYC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationsKYC_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VerificationsKYC_Utilisateurs_VerifieParId",
                        column: x => x.VerifieParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Virements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reference = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CompteSourceId = table.Column<int>(type: "integer", nullable: false),
                    CompteDestinationId = table.Column<int>(type: "integer", nullable: true),
                    EstInterbancaire = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BanqueDestination = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroCompteExterne = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    BeneficiaireExterne = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BIF"),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente"),
                    Motif = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ControleAML = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DateVirement = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateValidation = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Virements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Virements_Comptes_CompteDestinationId",
                        column: x => x.CompteDestinationId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Virements_Comptes_CompteSourceId",
                        column: x => x.CompteSourceId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Virements_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentsAcces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DateAcces = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentsAcces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentsAcces_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentsAcces_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EcheancesPret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PretId = table.Column<int>(type: "integer", nullable: false),
                    NumeroEcheance = table.Column<int>(type: "integer", nullable: false),
                    Capital = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Interet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente"),
                    Penalite = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcheancesPret", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcheancesPret_Prets_PretId",
                        column: x => x.PretId,
                        principalTable: "Prets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reference = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TypeOperation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CompteId = table.Column<int>(type: "integer", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    AgenceId = table.Column<int>(type: "integer", nullable: false),
                    VirementId = table.Column<int>(type: "integer", nullable: true),
                    PretId = table.Column<int>(type: "integer", nullable: true),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BIF"),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "EnAttente"),
                    DateOperation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateValidation = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Commentaire = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operations_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Comptes_CompteId",
                        column: x => x.CompteId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Prets_PretId",
                        column: x => x.PretId,
                        principalTable: "Prets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Virements_VirementId",
                        column: x => x.VirementId,
                        principalTable: "Virements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertesAML",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    OperationId = table.Column<int>(type: "integer", nullable: true),
                    TypeAlerte = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Niveau = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Ouverte"),
                    TraiteParId = table.Column<int>(type: "integer", nullable: true),
                    DateAlerte = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    DateTraitement = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertesAML", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertesAML_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertesAML_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlertesAML_Utilisateurs_TraiteParId",
                        column: x => x.TraiteParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ecritures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OperationId = table.Column<int>(type: "integer", nullable: false),
                    CompteId = table.Column<int>(type: "integer", nullable: true),
                    CompteComptableId = table.Column<int>(type: "integer", nullable: true),
                    AgenceId = table.Column<int>(type: "integer", nullable: true),
                    Sens = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Devise = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BIF"),
                    DateEcriture = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    Libelle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ecritures", x => x.Id);
                    table.CheckConstraint("CK_Ecritures_UnSeulCompte", "(\"CompteId\" IS NULL AND \"CompteComptableId\" IS NOT NULL) OR (\"CompteId\" IS NOT NULL AND \"CompteComptableId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_Ecritures_Agences_AgenceId",
                        column: x => x.AgenceId,
                        principalTable: "Agences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ecritures_ComptesComptables_CompteComptableId",
                        column: x => x.CompteComptableId,
                        principalTable: "ComptesComptables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ecritures_Comptes_CompteId",
                        column: x => x.CompteId,
                        principalTable: "Comptes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ecritures_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionsCartes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarteId = table.Column<int>(type: "integer", nullable: false),
                    OperationId = table.Column<int>(type: "integer", nullable: true),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TypeTransaction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MotifRefus = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DateTransaction = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionsCartes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionsCartes_Cartes_CarteId",
                        column: x => x.CarteId,
                        principalTable: "Cartes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionsCartes_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TypesPret",
                columns: new[] { "Id", "Code", "DureeMaxMois", "DureeMinMois", "EstActif", "Libelle", "MontantMax", "MontantMin", "TauxMax", "TauxMin" },
                values: new object[,]
                {
                    { 1, "PERSONNEL", 60, 3, true, "Prêt personnel", 10000000m, 100000m, 16m, 8m },
                    { 2, "EQUIPEMENT", 120, 6, true, "Prêt équipement", 50000000m, 500000m, 14m, 6m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agences_Code",
                table: "Agences",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_ClientId",
                table: "AlertesAML",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_OperationId",
                table: "AlertesAML",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_Statut",
                table: "AlertesAML",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_AlertesAML_TraiteParId",
                table: "AlertesAML",
                column: "TraiteParId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_AgenceId",
                table: "AuditLog",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_DateEvenement",
                table: "AuditLog",
                column: "DateEvenement");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UtilisateurId",
                table: "AuditLog",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_ClientId",
                table: "Cartes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_CompteId",
                table: "Cartes",
                column: "CompteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartes_NumeroCarte",
                table: "Cartes",
                column: "NumeroCarte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AgenceId",
                table: "Clients",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_NumeroClient",
                table: "Clients",
                column: "NumeroClient",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_NumeroCNI",
                table: "Clients",
                column: "NumeroCNI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_AgenceId",
                table: "Comptes",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_ClientId",
                table: "Comptes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_NumeroCompte",
                table: "Comptes",
                column: "NumeroCompte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comptes_TypeCompteId",
                table: "Comptes",
                column: "TypeCompteId");

            migrationBuilder.CreateIndex(
                name: "IX_ComptesComptables_Code",
                table: "ComptesComptables",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId_Categorie_Version",
                table: "Documents",
                columns: new[] { "ClientId", "Categorie", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DeposeParId",
                table: "Documents",
                column: "DeposeParId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Statut_DateSuppression",
                table: "Documents",
                columns: new[] { "Statut", "DateSuppression" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsAcces_DocumentId_DateAcces",
                table: "DocumentsAcces",
                columns: new[] { "DocumentId", "DateAcces" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentsAcces_UtilisateurId",
                table: "DocumentsAcces",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_EcheancesPret_DateEcheance",
                table: "EcheancesPret",
                column: "DateEcheance");

            migrationBuilder.CreateIndex(
                name: "IX_EcheancesPret_PretId_NumeroEcheance",
                table: "EcheancesPret",
                columns: new[] { "PretId", "NumeroEcheance" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_AgenceId",
                table: "Ecritures",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_CompteComptableId",
                table: "Ecritures",
                column: "CompteComptableId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_CompteId",
                table: "Ecritures",
                column: "CompteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_OperationId",
                table: "Ecritures",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationsUtilisateurs_UtilisateurId_DateLecture_DateCre~",
                table: "NotificationsUtilisateurs",
                columns: new[] { "UtilisateurId", "DateLecture", "DateCreation" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Code_Canal_Langue_Version",
                table: "NotificationTemplates",
                columns: new[] { "Code", "Canal", "Langue", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AgenceId",
                table: "Operations",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_CompteId",
                table: "Operations",
                column: "CompteId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_DateOperation",
                table: "Operations",
                column: "DateOperation");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_PretId",
                table: "Operations",
                column: "PretId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Reference",
                table: "Operations",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Statut",
                table: "Operations",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_UtilisateurId",
                table: "Operations",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_VirementId",
                table: "Operations",
                column: "VirementId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametres_AgenceId",
                table: "Parametres",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametres_Cle_AgenceId",
                table: "Parametres",
                columns: new[] { "Cle", "AgenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prets_ClientId",
                table: "Prets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_CompteDebitId",
                table: "Prets",
                column: "CompteDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_Reference",
                table: "Prets",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prets_Statut",
                table: "Prets",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_TypePretId",
                table: "Prets",
                column: "TypePretId");

            migrationBuilder.CreateIndex(
                name: "IX_Prets_ValideParId",
                table: "Prets",
                column: "ValideParId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesClaims_RoleId",
                table: "RolesClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionsUtilisateur_Token",
                table: "SessionsUtilisateur",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionsUtilisateur_UtilisateurId",
                table: "SessionsUtilisateur",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsCartes_CarteId_DateTransaction",
                table: "TransactionsCartes",
                columns: new[] { "CarteId", "DateTransaction" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsCartes_OperationId",
                table: "TransactionsCartes",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TypesCompte_Code",
                table: "TypesCompte",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TypesPret_Code",
                table: "TypesPret",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Utilisateurs",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_AgenceId",
                table: "Utilisateurs",
                column: "AgenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_ClientId",
                table: "Utilisateurs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Role",
                table: "Utilisateurs",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Utilisateurs",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UtilisateursClaims_UserId",
                table: "UtilisateursClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilisateursLogins_UserId",
                table: "UtilisateursLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilisateursRoles_RoleId",
                table: "UtilisateursRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_ClientId",
                table: "VerificationsKYC",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationsKYC_VerifieParId",
                table: "VerificationsKYC",
                column: "VerifieParId");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_CompteDestinationId",
                table: "Virements",
                column: "CompteDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_CompteSourceId",
                table: "Virements",
                column: "CompteSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_Reference",
                table: "Virements",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Virements_Statut",
                table: "Virements",
                column: "Statut");

            migrationBuilder.CreateIndex(
                name: "IX_Virements_UtilisateurId",
                table: "Virements",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertesAML");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "DocumentsAcces");

            migrationBuilder.DropTable(
                name: "EcheancesPret");

            migrationBuilder.DropTable(
                name: "Ecritures");

            migrationBuilder.DropTable(
                name: "NotificationsUtilisateurs");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "Parametres");

            migrationBuilder.DropTable(
                name: "RolesClaims");

            migrationBuilder.DropTable(
                name: "SessionsUtilisateur");

            migrationBuilder.DropTable(
                name: "TransactionsCartes");

            migrationBuilder.DropTable(
                name: "UtilisateursClaims");

            migrationBuilder.DropTable(
                name: "UtilisateursLogins");

            migrationBuilder.DropTable(
                name: "UtilisateursRoles");

            migrationBuilder.DropTable(
                name: "UtilisateursTokens");

            migrationBuilder.DropTable(
                name: "VerificationsKYC");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "ComptesComptables");

            migrationBuilder.DropTable(
                name: "Cartes");

            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Prets");

            migrationBuilder.DropTable(
                name: "Virements");

            migrationBuilder.DropTable(
                name: "TypesPret");

            migrationBuilder.DropTable(
                name: "Comptes");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "TypesCompte");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Agences");
        }
    }
}
