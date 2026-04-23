using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceDemoV1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderAndPaymentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Total",
                table: "Orders",
                newName: "RankDiscount");

            migrationBuilder.AddColumn<string>(
                name: "PayOsOrderId",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CouponDiscount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalTotal",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayOsOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CouponDiscount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinalTotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PromotionDiscount",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "RankDiscount",
                table: "Orders",
                newName: "Total");
        }
    }
}
