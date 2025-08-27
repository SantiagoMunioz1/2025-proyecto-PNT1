using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.C.Data.Migrations
{
    /// <inheritdoc />
    public partial class IndicesUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Prestaciones_Nombre",
                table: "Prestaciones",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_DNI",
                table: "Personas",
                column: "DNI",
                unique: true,
                filter: "[DNI] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_NumeroMatricula",
                table: "Matriculas",
                column: "NumeroMatricula",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Prestaciones_Nombre",
                table: "Prestaciones");

            migrationBuilder.DropIndex(
                name: "IX_Personas_DNI",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_NumeroMatricula",
                table: "Matriculas");
        }
    }
}
