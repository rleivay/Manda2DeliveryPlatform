using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixShippingAddressCustomerRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingAddresses_Customers_CustomerId1",
                schema: "core",
                table: "ShippingAddresses");

            migrationBuilder.DropIndex(
                name: "IX_ShippingAddresses_CustomerId1",
                schema: "core",
                table: "ShippingAddresses");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                schema: "core",
                table: "ShippingAddresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId1",
                schema: "core",
                table: "ShippingAddresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingAddresses_CustomerId1",
                schema: "core",
                table: "ShippingAddresses",
                column: "CustomerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingAddresses_Customers_CustomerId1",
                schema: "core",
                table: "ShippingAddresses",
                column: "CustomerId1",
                principalSchema: "core",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
