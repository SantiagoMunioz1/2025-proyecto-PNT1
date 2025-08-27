using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.C.Data.Migrations
{
    /// <inheritdoc />
    public partial class FormularioRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Formularios_Personas_Id",
                table: "Formularios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Formularios",
                table: "Formularios");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Formularios");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Formularios",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Formularios",
                table: "Formularios",
                column: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Formularios",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DNI",
                table: "Formularios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Formularios",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Formularios_PacienteId",
                table: "Formularios",
                column: "PacienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Formularios_Personas_PacienteId",
                table: "Formularios",
                column: "PacienteId",
                principalTable: "Personas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Formularios_Personas_PacienteId",
                table: "Formularios");

            migrationBuilder.DropIndex(
                name: "IX_Formularios_PacienteId",
                table: "Formularios");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Formularios");

            migrationBuilder.DropColumn(
                name: "DNI",
                table: "Formularios");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Formularios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Formularios",
                table: "Formularios");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Formularios");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Formularios",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Formularios",
                table: "Formularios",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Formularios_Personas_Id",
                table: "Formularios",
                column: "Id",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
