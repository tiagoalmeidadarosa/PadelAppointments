using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PadelAppointments.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendasAndOrganizationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Courts_CourtId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Courts_CourtId",
                table: "Schedules");

            migrationBuilder.DropTable(
                name: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_CourtId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_Date_Time_CourtId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CourtId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CourtId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "CourtId",
                table: "Appointments");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "AgendaId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    StartsAt = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndsAt = table.Column<TimeSpan>(type: "time", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agendas_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AgendaId",
                table: "Appointments",
                column: "AgendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendas_OrganizationId",
                table: "Agendas",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Agendas_AgendaId",
                table: "Appointments",
                column: "AgendaId",
                principalTable: "Agendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Agendas_AgendaId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Agendas");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AgendaId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AgendaId",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "CourtId",
                table: "Schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CourtId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Courts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Courts",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Quadra1" },
                    { 2, "Quadra2" },
                    { 3, "Quadra3" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CourtId",
                table: "Schedules",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_Date_Time_CourtId",
                table: "Schedules",
                columns: new[] { "Date", "Time", "CourtId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CourtId",
                table: "Appointments",
                column: "CourtId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Courts_CourtId",
                table: "Appointments",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Courts_CourtId",
                table: "Schedules",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
