using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class Chapitre14DocumentsEtAcces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    DeposeParId = table.Column<int>(type: "int", nullable: true),
                    Categorie = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NomOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CleStockage = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TypeMime = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TailleOctets = table.Column<long>(type: "bigint", nullable: false),
                    HashSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Actif"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateArchivage = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateSuppression = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "DocumentsAcces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DateAcces = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId_Categorie_Version",
                table: "Documents",
                columns: new[] { "ClientId", "Categorie", "Version" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentsAcces");

            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
