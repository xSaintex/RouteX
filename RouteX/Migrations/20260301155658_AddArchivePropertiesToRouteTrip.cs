using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteX.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivePropertiesToRouteTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "RouteTrips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "RouteTrips",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "RouteTrips");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "RouteTrips");
        }
    }
}
