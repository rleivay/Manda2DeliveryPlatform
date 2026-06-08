using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryFeeAuditToSubOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryDistanceKm",
                schema: "core",
                table: "SubOrders",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryDistancePct",
                schema: "core",
                table: "SubOrders",
                type: "decimal(8,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotFeeBase",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotFeeKmIncluidos",
                schema: "core",
                table: "SubOrders",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotFeePorKm",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotTotalGroupDistanceKm",
                schema: "core",
                table: "SubOrders",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotTotalGroupFee",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDistanceKm",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryDistancePct",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "SnapshotFeeBase",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "SnapshotFeeKmIncluidos",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "SnapshotFeePorKm",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "SnapshotTotalGroupDistanceKm",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "SnapshotTotalGroupFee",
                schema: "core",
                table: "SubOrders");
        }
    }
}
