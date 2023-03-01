using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelAppointments.Migrations
{
    /// <inheritdoc />
    public partial class CreateUniqueIndexToDateTimeAndCourtId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_Time",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_Time_CourtId",
                table: "Appointments",
                columns: new[] { "Date", "Time", "CourtId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_Time_CourtId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_Time",
                table: "Appointments",
                columns: new[] { "Date", "Time" },
                unique: true);
        }
    }
}
