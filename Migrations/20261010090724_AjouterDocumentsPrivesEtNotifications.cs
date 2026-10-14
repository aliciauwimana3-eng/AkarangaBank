using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class AjouterDocumentsPrivesEtNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomDocumentOriginal",
                table: "VerificationsKYC",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TailleDocument",
                table: "VerificationsKYC",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeMimeDocument",
                table: "VerificationsKYC",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationsUtilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DateLecture = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_NotificationsUtilisateurs_UtilisateurId_DateLecture_DateCreation",
                table: "NotificationsUtilisateurs",
                columns: new[] { "UtilisateurId", "DateLecture", "DateCreation" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationsUtilisateurs");

            migrationBuilder.DropColumn(
                name: "NomDocumentOriginal",
                table: "VerificationsKYC");

            migrationBuilder.DropColumn(
                name: "TailleDocument",
                table: "VerificationsKYC");

            migrationBuilder.DropColumn(
                name: "TypeMimeDocument",
                table: "VerificationsKYC");
        }
    }
}
