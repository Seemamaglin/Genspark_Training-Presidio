using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardingDroppingPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoardingPoint",
                table: "Buses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DroppingPoint",
                table: "Buses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardingPoint",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "DroppingPoint",
                table: "Buses");
        }
    }
}
