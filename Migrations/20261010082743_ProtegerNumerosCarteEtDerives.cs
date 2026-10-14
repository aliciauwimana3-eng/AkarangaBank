using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class ProtegerNumerosCarteEtDerives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DerniersChiffres",
                table: "Cartes",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroCarteChiffre",
                table: "Cartes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Cartes SET DerniersChiffres = RIGHT(REPLACE(REPLACE(NumeroCarte, '-', ''), ' ', ''), 4)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DerniersChiffres",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "NumeroCarteChiffre",
                table: "Cartes");
        }
    }
}
