using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EntregaB_CatalogoComisiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPct",
                schema: "cfg",
                table: "ProductCategories",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPct",
                schema: "core",
                table: "Merchants",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "aud",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresImmediateAttention",
                schema: "aud",
                table: "AuditLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalSchema: "cfg",
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MerchantProducts",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CommissionSource = table.Column<int>(type: "int", nullable: false),
                    CommissionPctOverride = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ResolvedCommissionPct = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ResolvedCommissionSource = table.Column<int>(type: "int", nullable: true),
                    ShowSamePriceLabel = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantProducts_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "core",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MerchantProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "core",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProducts_MerchantId_ProductId",
                schema: "core",
                table: "MerchantProducts",
                columns: new[] { "MerchantId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProducts_ProductId",
                schema: "core",
                table: "MerchantProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId",
                schema: "core",
                table: "Products",
                column: "ProductCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantProducts",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "CommissionPct",
                schema: "cfg",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "CommissionPct",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "aud",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RequiresImmediateAttention",
                schema: "aud",
                table: "AuditLogs");
        }
    }
}
