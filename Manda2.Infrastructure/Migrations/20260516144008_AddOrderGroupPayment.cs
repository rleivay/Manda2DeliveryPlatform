using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderGroupPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressText",
                schema: "core",
                table: "Merchants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                schema: "core",
                table: "Merchants",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                schema: "core",
                table: "Merchants",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "OrderGroupPayments",
                schema: "fin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderGroupId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Authorization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProofUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfirmedByUserId = table.Column<int>(type: "int", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderGroupPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderGroupPayments_OrderGroups_OrderGroupId",
                        column: x => x.OrderGroupId,
                        principalSchema: "ord",
                        principalTable: "OrderGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderGroupPayments_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "cfg",
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroupPayment_Reference",
                schema: "fin",
                table: "OrderGroupPayments",
                column: "Reference");

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroupPayments_OrderGroupId",
                schema: "fin",
                table: "OrderGroupPayments",
                column: "OrderGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroupPayments_PaymentMethodId",
                schema: "fin",
                table: "OrderGroupPayments",
                column: "PaymentMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderGroupPayments",
                schema: "fin");

            migrationBuilder.DropColumn(
                name: "AddressText",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "core",
                table: "Merchants");
        }
    }
}
