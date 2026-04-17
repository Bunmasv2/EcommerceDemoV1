using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceDemoV1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPromotionRuleAndUserToStronglyTyped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionJson",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "ConditionJson",
                table: "PromotionRules");

            migrationBuilder.RenameColumn(
                name: "RuleType",
                table: "PromotionRules",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "MemberRank",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Bronze",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "ApplyToCategoryId",
                table: "PromotionRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApplyToProductVariantId",
                table: "PromotionRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PromotionRules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "PromotionRules",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FreeQuantity",
                table: "PromotionRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinQuantity",
                table: "PromotionRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AppliedCouponId",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRules_ApplyToCategoryId",
                table: "PromotionRules",
                column: "ApplyToCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRules_ApplyToProductVariantId",
                table: "PromotionRules",
                column: "ApplyToProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_AppliedCouponId",
                table: "Carts",
                column: "AppliedCouponId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Coupons_AppliedCouponId",
                table: "Carts",
                column: "AppliedCouponId",
                principalTable: "Coupons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionRules_Categories_ApplyToCategoryId",
                table: "PromotionRules",
                column: "ApplyToCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionRules_ProductVariants_ApplyToProductVariantId",
                table: "PromotionRules",
                column: "ApplyToProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Coupons_AppliedCouponId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionRules_Categories_ApplyToCategoryId",
                table: "PromotionRules");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionRules_ProductVariants_ApplyToProductVariantId",
                table: "PromotionRules");

            migrationBuilder.DropIndex(
                name: "IX_PromotionRules_ApplyToCategoryId",
                table: "PromotionRules");

            migrationBuilder.DropIndex(
                name: "IX_PromotionRules_ApplyToProductVariantId",
                table: "PromotionRules");

            migrationBuilder.DropIndex(
                name: "IX_Carts_AppliedCouponId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ApplyToCategoryId",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "ApplyToProductVariantId",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "FreeQuantity",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "MinQuantity",
                table: "PromotionRules");

            migrationBuilder.DropColumn(
                name: "AppliedCouponId",
                table: "Carts");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "PromotionRules",
                newName: "RuleType");

            migrationBuilder.AlterColumn<string>(
                name: "MemberRank",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Bronze");

            migrationBuilder.AddColumn<string>(
                name: "ActionJson",
                table: "PromotionRules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConditionJson",
                table: "PromotionRules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
