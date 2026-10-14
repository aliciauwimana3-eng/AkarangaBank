using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class Chapitre14CanauxEtPlanification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PushToken",
                table: "Utilisateurs",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Langue",
                table: "NotificationsUtilisateurs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "fr-BI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PushToken",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "Langue",
                table: "NotificationsUtilisateurs");
        }
    }
}
