using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BusBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatorStops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DroppingStop",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupStop",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OperatorStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusOperatorId = table.Column<int>(type: "integer", nullable: false),
                    StopName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorStops_BusOperators_BusOperatorId",
                        column: x => x.BusOperatorId,
                        principalTable: "BusOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorStops_BusOperatorId",
                table: "OperatorStops",
                column: "BusOperatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatorStops");

            migrationBuilder.DropColumn(
                name: "DroppingStop",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PickupStop",
                table: "Bookings");
        }
    }
}
