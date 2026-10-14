using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class Chapitre14TemplatesNotificationsEtDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Canal",
                table: "NotificationsUtilisateurs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Interne");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEnvoi",
                table: "NotificationsUtilisateurs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Erreur",
                table: "NotificationsUtilisateurs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statut",
                table: "NotificationsUtilisateurs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Envoyee");

            migrationBuilder.AddColumn<string>(
                name: "TemplateCode",
                table: "NotificationsUtilisateurs",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tentatives",
                table: "NotificationsUtilisateurs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Langue = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Sujet = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContenuHtml = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: true),
                    ContenuTexte = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EstActive = table.Column<bool>(type: "bit", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Code_Canal_Langue_Version",
                table: "NotificationTemplates",
                columns: new[] { "Code", "Canal", "Langue", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "Canal",
                table: "NotificationsUtilisateurs");

            migrationBuilder.DropColumn(
                name: "DateEnvoi",
                table: "NotificationsUtilisateurs");

            migrationBuilder.DropColumn(
                name: "Erreur",
                table: "NotificationsUtilisateurs");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "NotificationsUtilisateurs");

            migrationBuilder.DropColumn(
                name: "TemplateCode",
                table: "NotificationsUtilisateurs");

            migrationBuilder.DropColumn(
                name: "Tentatives",
                table: "NotificationsUtilisateurs");
        }
    }
}
