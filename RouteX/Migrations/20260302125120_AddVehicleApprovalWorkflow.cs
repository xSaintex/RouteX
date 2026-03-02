using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteX.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedByUserEmail",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedByUserId",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserEmail",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPendingApproval",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedByUserEmail",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "AddedByUserId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserEmail",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsPendingApproval",
                table: "Vehicles");
        }
    }
}
