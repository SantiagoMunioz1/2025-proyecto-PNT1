using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.C.Data.Migrations
{
    /// <inheritdoc />
    public partial class IndiceUniqueConstraintCobertura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Coberturas_NumeroCredencial",
                table: "Coberturas",
                column: "NumeroCredencial",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Coberturas_NumeroCredencial",
                table: "Coberturas");
        }
    }
}
