using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelAppointments.Migrations
{
    /// <inheritdoc />
    public partial class CreateUniqueIndexToDateAndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_Time",
                table: "Appointments",
                columns: new[] { "Date", "Time" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_Time",
                table: "Appointments");
        }
    }
}
