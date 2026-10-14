using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class VirementsInterbancairesEtSeuilsConformite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CompteDestinationId",
                table: "Virements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BanqueDestination",
                table: "Virements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaireExterne",
                table: "Virements",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstInterbancaire",
                table: "Virements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCompteExterne",
                table: "Virements",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BanqueDestination",
                table: "Virements");

            migrationBuilder.DropColumn(
                name: "BeneficiaireExterne",
                table: "Virements");

            migrationBuilder.DropColumn(
                name: "EstInterbancaire",
                table: "Virements");

            migrationBuilder.DropColumn(
                name: "NumeroCompteExterne",
                table: "Virements");

            migrationBuilder.AlterColumn<int>(
                name: "CompteDestinationId",
                table: "Virements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
