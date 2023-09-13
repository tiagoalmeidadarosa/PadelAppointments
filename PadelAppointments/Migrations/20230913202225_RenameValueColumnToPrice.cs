using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadelAppointments.Migrations
{
    /// <inheritdoc />
    public partial class RenameValueColumnToPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ItemsConsumed",
                newName: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ItemsConsumed",
                newName: "Value");
        }
    }
}
