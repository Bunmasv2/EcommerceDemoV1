using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceDemoV1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftProductToPromotionRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GiftProductVariantId",
                table: "PromotionRules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRules_GiftProductVariantId",
                table: "PromotionRules",
                column: "GiftProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionRules_ProductVariants_GiftProductVariantId",
                table: "PromotionRules",
                column: "GiftProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionRules_ProductVariants_GiftProductVariantId",
                table: "PromotionRules");

            migrationBuilder.DropIndex(
                name: "IX_PromotionRules_GiftProductVariantId",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "GiftProductVariantId",
                table: "PromotionRules");
        }
    }
}
