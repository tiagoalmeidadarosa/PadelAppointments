using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelAppointments.Migrations
{
    /// <inheritdoc />
    public partial class NewTableStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Courts_CourtId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Recurrences_RecurrenceId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Recurrences");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_Time_CourtId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_RecurrenceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RecurrenceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Appointments");

            migrationBuilder.AlterColumn<int>(
                name: "CourtId",
                table: "Appointments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "HasRecurrence",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    CourtId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedules_Courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_AppointmentId",
                table: "Schedules",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CourtId",
                table: "Schedules",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_Date_Time_CourtId",
                table: "Schedules",
                columns: new[] { "Date", "Time", "CourtId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Courts_CourtId",
                table: "Appointments",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Courts_CourtId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropColumn(
                name: "HasRecurrence",
                table: "Appointments");

            migrationBuilder.AlterColumn<int>(
                name: "CourtId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Time",
                table: "Appointments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateTable(
                name: "Recurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recurrences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_Time_CourtId",
                table: "Appointments",
                columns: new[] { "Date", "Time", "CourtId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_RecurrenceId",
                table: "Appointments",
                column: "RecurrenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Courts_CourtId",
                table: "Appointments",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Recurrences_RecurrenceId",
                table: "Appointments",
                column: "RecurrenceId",
                principalTable: "Recurrences",
                principalColumn: "Id");
        }
    }
}
