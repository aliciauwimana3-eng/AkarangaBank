using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class AjouterTransactionsEtPlafondsCartes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PlafondEnLigneJournalier",
                table: "Cartes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 300000m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlafondPaiementJournalier",
                table: "Cartes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 1000000m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlafondRetraitJournalier",
                table: "Cartes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 500000m);

            migrationBuilder.CreateTable(
                name: "TransactionsCartes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarteId = table.Column<int>(type: "int", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: true),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TypeTransaction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotifRefus = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DateTransaction = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsCartes_CarteId_DateTransaction",
                table: "TransactionsCartes",
                columns: new[] { "CarteId", "DateTransaction" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsCartes_OperationId",
                table: "TransactionsCartes",
                column: "OperationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionsCartes");

            migrationBuilder.DropColumn(
                name: "PlafondEnLigneJournalier",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "PlafondPaiementJournalier",
                table: "Cartes");

            migrationBuilder.DropColumn(
                name: "PlafondRetraitJournalier",
                table: "Cartes");
        }
    }
}
