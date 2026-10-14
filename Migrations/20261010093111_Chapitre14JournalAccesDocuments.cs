using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class Chapitre14JournalAccesDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_ClientId_Categorie_Version",
                table: "Documents");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId_Categorie_Version",
                table: "Documents",
                columns: new[] { "ClientId", "Categorie", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_ClientId_Categorie_Version",
                table: "Documents");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId_Categorie_Version",
                table: "Documents",
                columns: new[] { "ClientId", "Categorie", "Version" });
        }
    }
}
