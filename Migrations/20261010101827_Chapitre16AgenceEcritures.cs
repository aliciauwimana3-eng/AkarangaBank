using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkarangaBank.Web.Migrations
{
    /// <inheritdoc />
    public partial class Chapitre16AgenceEcritures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgenceId",
                table: "Ecritures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ecritures_AgenceId",
                table: "Ecritures",
                column: "AgenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ecritures_Agences_AgenceId",
                table: "Ecritures",
                column: "AgenceId",
                principalTable: "Agences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ecritures_Agences_AgenceId",
                table: "Ecritures");

            migrationBuilder.DropIndex(
                name: "IX_Ecritures_AgenceId",
                table: "Ecritures");

            migrationBuilder.DropColumn(
                name: "AgenceId",
                table: "Ecritures");
        }
    }
}
