using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class AjouterTypesDePret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypePretId",
                table: "Prets",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "TypesPret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MontantMin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantMax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DureeMinMois = table.Column<int>(type: "int", nullable: false),
                    DureeMaxMois = table.Column<int>(type: "int", nullable: false),
                    TauxMin = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TauxMax = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesPret", x => x.Id);
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
                name: "IX_Prets_TypePretId",
                table: "Prets",
                column: "TypePretId");

            migrationBuilder.CreateIndex(
                name: "IX_TypesPret_Code",
                table: "TypesPret",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Prets_TypesPret_TypePretId",
                table: "Prets",
                column: "TypePretId",
                principalTable: "TypesPret",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prets_TypesPret_TypePretId",
                table: "Prets");

            migrationBuilder.DropTable(
                name: "TypesPret");

            migrationBuilder.DropIndex(
                name: "IX_Prets_TypePretId",
                table: "Prets");

            migrationBuilder.DropColumn(
                name: "TypePretId",
                table: "Prets");
        }
    }
}
