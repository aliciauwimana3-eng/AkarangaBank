using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class AjouterComptesComptables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CompteId",
                table: "Ecritures",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CompteComptableId",
                table: "Ecritures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComptesComptables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Devise = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComptesComptables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_CompteComptableId",
                table: "Ecritures",
                column: "CompteComptableId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ecritures_UnSeulCompte",
                table: "Ecritures",
                sql: "(CompteId IS NULL AND CompteComptableId IS NOT NULL) OR (CompteId IS NOT NULL AND CompteComptableId IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ComptesComptables_Code",
                table: "ComptesComptables",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ecritures_ComptesComptables_CompteComptableId",
                table: "Ecritures",
                column: "CompteComptableId",
                principalTable: "ComptesComptables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ecritures_ComptesComptables_CompteComptableId",
                table: "Ecritures");

            migrationBuilder.DropTable(
                name: "ComptesComptables");

            migrationBuilder.DropIndex(
                name: "IX_Ecritures_CompteComptableId",
                table: "Ecritures");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ecritures_UnSeulCompte",
                table: "Ecritures");

            migrationBuilder.DropColumn(
                name: "CompteComptableId",
                table: "Ecritures");

            migrationBuilder.AlterColumn<int>(
                name: "CompteId",
                table: "Ecritures",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
