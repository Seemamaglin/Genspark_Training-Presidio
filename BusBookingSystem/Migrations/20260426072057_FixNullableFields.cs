using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixNullableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BusSchedule_ScheduleId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_AspNetUsers_OperatorId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_BusOperators_BusOperatorId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_BusSchedule_ScheduleId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "DepartureTime",
                table: "Buses");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "Seats",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "SeatLayout",
                table: "Buses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OperatorId",
                table: "Buses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "BusOperatorId",
                table: "Buses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "ArrivalTime",
                table: "Buses",
                type: "interval",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "Bookings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BusSchedule_ScheduleId",
                table: "Bookings",
                column: "ScheduleId",
                principalTable: "BusSchedule",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_AspNetUsers_OperatorId",
                table: "Buses",
                column: "OperatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_BusOperators_BusOperatorId",
                table: "Buses",
                column: "BusOperatorId",
                principalTable: "BusOperators",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_BusSchedule_ScheduleId",
                table: "Seats",
                column: "ScheduleId",
                principalTable: "BusSchedule",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BusSchedule_ScheduleId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_AspNetUsers_OperatorId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_BusOperators_BusOperatorId",
                table: "Buses");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_BusSchedule_ScheduleId",
                table: "Seats");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "Seats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SeatLayout",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OperatorId",
                table: "Buses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BusOperatorId",
                table: "Buses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "ArrivalTime",
                table: "Buses",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "interval",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DepartureTime",
                table: "Buses",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleId",
                table: "Bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BusSchedule_ScheduleId",
                table: "Bookings",
                column: "ScheduleId",
                principalTable: "BusSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_AspNetUsers_OperatorId",
                table: "Buses",
                column: "OperatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_BusOperators_BusOperatorId",
                table: "Buses",
                column: "BusOperatorId",
                principalTable: "BusOperators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_BusSchedule_ScheduleId",
                table: "Seats",
                column: "ScheduleId",
                principalTable: "BusSchedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
