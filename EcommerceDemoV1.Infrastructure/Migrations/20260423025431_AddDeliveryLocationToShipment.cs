using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceDemoV1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryLocationToShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeliveryLatitude",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLongitude",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryLatitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DeliveryLongitude",
                table: "Shipments");
        }
    }
}
