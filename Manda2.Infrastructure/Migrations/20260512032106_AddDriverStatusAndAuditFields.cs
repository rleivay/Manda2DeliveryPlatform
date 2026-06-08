using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverStatusAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                schema: "ord",
                table: "OrderGroups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "core",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                schema: "aud",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformedBy",
                schema: "aud",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                schema: "ord",
                table: "OrderGroups");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Details",
                schema: "aud",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PerformedBy",
                schema: "aud",
                table: "AuditLogs");
        }
    }
}
