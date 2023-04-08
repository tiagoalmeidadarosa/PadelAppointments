using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelAppointments.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Appointments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Appointments");
        }
    }
}
