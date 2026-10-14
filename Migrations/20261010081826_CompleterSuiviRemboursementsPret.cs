using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class CompleterSuiviRemboursementsPret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Commentaire",
                table: "Prets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantRembourse",
                table: "Prets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotifRejet",
                table: "Prets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Penalite",
                table: "EcheancesPret",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commentaire",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "MontantRembourse",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "MotifRejet",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "Penalite",
                table: "EcheancesPret");
        }
    }
}
