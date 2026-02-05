using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacacionesBancodeAlimentos.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calculos",
                columns: table => new
                {
                    indice = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    anioMin = table.Column<int>(type: "int", nullable: false),
                    anioMax = table.Column<int>(type: "int", nullable: false),
                    dias = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculos", x => x.indice);
                });

            migrationBuilder.CreateTable(
                name: "diccionarioAsuetos",
                columns: table => new
                {
                    idFecha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diccionarioAsuetos", x => x.idFecha);
                });

            migrationBuilder.CreateTable(
                name: "solicitudes",
                columns: table => new
                {
                    idSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    empleadoId = table.Column<int>(type: "int", nullable: false),
                    formato = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    estatus = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    fechaPeticion = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudes", x => x.idSolicitud);
                });

            migrationBuilder.CreateTable(
                name: "vacaciones",
                columns: table => new
                {
                    empleadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    anio = table.Column<int>(type: "int", nullable: false),
                    diasTotales = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacaciones", x => x.empleadoId);
                });

            migrationBuilder.CreateTable(
                name: "asuetosFechas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaId = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asuetosFechas", x => x.id);
                    table.ForeignKey(
                        name: "FK_asuetosFechas_diccionarioAsuetos_fechaId",
                        column: x => x.fechaId,
                        principalTable: "diccionarioAsuetos",
                        principalColumn: "idFecha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solicitudesFechas",
                columns: table => new
                {
                    solicitudId = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudesFechas", x => x.solicitudId);
                    table.ForeignKey(
                        name: "FK_solicitudesFechas_solicitudes_solicitudId",
                        column: x => x.solicitudId,
                        principalTable: "solicitudes",
                        principalColumn: "idSolicitud",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asuetosFechas_fechaId",
                table: "asuetosFechas",
                column: "fechaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asuetosFechas");

            migrationBuilder.DropTable(
                name: "calculos");

            migrationBuilder.DropTable(
                name: "solicitudesFechas");

            migrationBuilder.DropTable(
                name: "vacaciones");

            migrationBuilder.DropTable(
                name: "diccionarioAsuetos");

            migrationBuilder.DropTable(
                name: "solicitudes");
        }
    }
}
