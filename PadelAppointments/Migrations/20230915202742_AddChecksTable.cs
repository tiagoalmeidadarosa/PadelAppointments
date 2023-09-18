using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelAppointments.Migrations
{
    /// <inheritdoc />
    public partial class AddChecksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsConsumed_Appointments_AppointmentId",
                table: "ItemsConsumed");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "ItemsConsumed",
                newName: "CheckId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsConsumed_AppointmentId",
                table: "ItemsConsumed",
                newName: "IX_ItemsConsumed_CheckId");

            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "ItemsConsumed",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Checks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PriceDividedBy = table.Column<int>(type: "int", nullable: false),
                    PricePaidFor = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checks_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checks_AppointmentId",
                table: "Checks",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_Date_AppointmentId",
                table: "Checks",
                columns: new[] { "Date", "AppointmentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsConsumed_Checks_CheckId",
                table: "ItemsConsumed",
                column: "CheckId",
                principalTable: "Checks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsConsumed_Checks_CheckId",
                table: "ItemsConsumed");

            migrationBuilder.DropTable(
                name: "Checks");

            migrationBuilder.DropColumn(
                name: "Paid",
                table: "ItemsConsumed");

            migrationBuilder.RenameColumn(
                name: "CheckId",
                table: "ItemsConsumed",
                newName: "AppointmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsConsumed_CheckId",
                table: "ItemsConsumed",
                newName: "IX_ItemsConsumed_AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsConsumed_Appointments_AppointmentId",
                table: "ItemsConsumed",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
