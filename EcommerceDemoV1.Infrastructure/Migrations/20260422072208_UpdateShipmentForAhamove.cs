using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceDemoV1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipmentForAhamove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "Shipments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverPhone",
                table: "Shipments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceId",
                table: "Shipments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Shipments",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Shipments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DriverPhone",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Shipments");
        }
    }
}
