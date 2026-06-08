using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewAddreesFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApartmentNumber",
                schema: "core",
                table: "ShippingAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                schema: "core",
                table: "ShippingAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNotes",
                schema: "core",
                table: "ShippingAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneCountryCode",
                schema: "core",
                table: "ShippingAddresses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentNumber",
                schema: "core",
                table: "ShippingAddresses");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                schema: "core",
                table: "ShippingAddresses");

            migrationBuilder.DropColumn(
                name: "DeliveryNotes",
                schema: "core",
                table: "ShippingAddresses");

            migrationBuilder.DropColumn(
                name: "PhoneCountryCode",
                schema: "core",
                table: "ShippingAddresses");
        }
    }
}
