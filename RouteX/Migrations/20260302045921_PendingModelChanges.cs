using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteX.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalMileage",
                table: "Vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalMileage",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
