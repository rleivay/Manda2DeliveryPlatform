using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Merchant_AddCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "core",
                table: "Merchants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                schema: "core",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                schema: "core",
                table: "Merchants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MerchantCategoryId",
                schema: "core",
                table: "Merchants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_MerchantCategoryId",
                schema: "core",
                table: "Merchants",
                column: "MerchantCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Merchants_MerchantCategories_MerchantCategoryId",
                schema: "core",
                table: "Merchants",
                column: "MerchantCategoryId",
                principalSchema: "cfg",
                principalTable: "MerchantCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Merchants_MerchantCategories_MerchantCategoryId",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_MerchantCategoryId",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "MerchantCategoryId",
                schema: "core",
                table: "Merchants");
        }
    }
}
