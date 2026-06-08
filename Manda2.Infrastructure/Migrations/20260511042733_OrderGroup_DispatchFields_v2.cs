using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderGroup_DispatchFields_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DriverAcceptedAtUtc",
                schema: "ord",
                table: "OrderGroups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverLatAtAcceptance",
                schema: "ord",
                table: "OrderGroups",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverLonAtAcceptance",
                schema: "ord",
                table: "OrderGroups",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                schema: "ord",
                table: "OrderGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodName",
                schema: "ord",
                table: "OrderGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroups_PaymentMethodId",
                schema: "ord",
                table: "OrderGroups",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderGroups_PaymentMethods_PaymentMethodId",
                schema: "ord",
                table: "OrderGroups",
                column: "PaymentMethodId",
                principalSchema: "cfg",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderGroups_PaymentMethods_PaymentMethodId",
                schema: "ord",
                table: "OrderGroups");

            migrationBuilder.DropIndex(
                name: "IX_OrderGroups_PaymentMethodId",
                schema: "ord",
                table: "OrderGroups");

            migrationBuilder.DropColumn(
                name: "DriverAcceptedAtUtc",
                schema: "ord",
                table: "OrderGroups");

            migrationBuilder.DropColumn(
                name: "DriverLatAtAcceptance",
                schema: "ord",
                table: "OrderGroups");

            migrationBuilder.DropColumn(
                name: "DriverLonAtAcceptance",
                schema: "ord",
                table: "OrderGroups");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                schema: "ord",
                table: "OrderGroups");

            migrationBuilder.DropColumn(
                name: "PaymentMethodName",
                schema: "ord",
                table: "OrderGroups");
        }
    }
}
