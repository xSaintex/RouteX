using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteX.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchesSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "RouteTrips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId1",
                table: "RouteTrips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId1",
                table: "RouteTrips",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    CoverageRadiusKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OperatingHours = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ServiceAreas = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_BranchId",
                table: "Vehicles",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteTrips_BranchId",
                table: "RouteTrips",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteTrips_BranchId1",
                table: "RouteTrips",
                column: "BranchId1");

            migrationBuilder.CreateIndex(
                name: "IX_RouteTrips_VehicleId1",
                table: "RouteTrips",
                column: "VehicleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteTrips_Branches_BranchId",
                table: "RouteTrips",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RouteTrips_Branches_BranchId1",
                table: "RouteTrips",
                column: "BranchId1",
                principalTable: "Branches",
                principalColumn: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouteTrips_Vehicles_VehicleId1",
                table: "RouteTrips",
                column: "VehicleId1",
                principalTable: "Vehicles",
                principalColumn: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Branches_BranchId",
                table: "Vehicles",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouteTrips_Branches_BranchId",
                table: "RouteTrips");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteTrips_Branches_BranchId1",
                table: "RouteTrips");

            migrationBuilder.DropForeignKey(
                name: "FK_RouteTrips_Vehicles_VehicleId1",
                table: "RouteTrips");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Branches_BranchId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_BranchId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_RouteTrips_BranchId",
                table: "RouteTrips");

            migrationBuilder.DropIndex(
                name: "IX_RouteTrips_BranchId1",
                table: "RouteTrips");

            migrationBuilder.DropIndex(
                name: "IX_RouteTrips_VehicleId1",
                table: "RouteTrips");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RouteTrips");

            migrationBuilder.DropColumn(
                name: "BranchId1",
                table: "RouteTrips");

            migrationBuilder.DropColumn(
                name: "VehicleId1",
                table: "RouteTrips");
        }
    }
}
