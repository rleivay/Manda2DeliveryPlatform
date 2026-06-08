using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BeforeMAUIStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultPreparationMinutes",
                schema: "core",
                table: "Merchants",
                type: "int",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<int>(
                name: "PreparationMinutes",
                schema: "core",
                table: "MerchantProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DistanceWeight",
                schema: "dsp",
                table: "DispatchConfigs",
                type: "decimal(18,4)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FairnessWeight",
                schema: "dsp",
                table: "DispatchConfigs",
                type: "decimal(18,4)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MerchantPrepToleranceMinutes",
                schema: "dsp",
                table: "DispatchConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ReliabilityWeight",
                schema: "dsp",
                table: "DispatchConfigs",
                type: "decimal(18,4)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RescueModeThresholdMinutes",
                schema: "dsp",
                table: "DispatchConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SlaWeight",
                schema: "dsp",
                table: "DispatchConfigs",
                type: "decimal(18,4)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UrgentRoundTimeoutSeconds",
                schema: "dsp",
                table: "DispatchConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProjectedReadyAtUtc",
                schema: "dsp",
                table: "DispatchAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strategy",
                schema: "dsp",
                table: "DispatchAttempts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "dsp",
                table: "DispatchConfigs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DistanceWeight", "FairnessWeight", "MerchantPrepToleranceMinutes", "ReliabilityWeight", "RescueModeThresholdMinutes", "SlaWeight", "UrgentRoundTimeoutSeconds" },
                values: new object[] { 0.30m, 0.25m, 3, 0.20m, 10, 0.25m, 60 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPreparationMinutes",
                schema: "core",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "PreparationMinutes",
                schema: "core",
                table: "MerchantProducts");

            migrationBuilder.DropColumn(
                name: "DistanceWeight",
                schema: "dsp",
                table: "DispatchConfigs");

            migrationBuilder.DropColumn(
                name: "FairnessWeight",
                schema: "dsp",
                table: "DispatchConfigs");

            migrationBuilder.DropColumn(
                name: "MerchantPrepToleranceMinutes",
                schema: "dsp",
                table: "DispatchConfigs");

            migrationBuilder.DropColumn(
                name: "ReliabilityWeight",
                schema: "dsp",
                table: "DispatchConfigs");

            migrationBuilder.DropColumn(
                name: "RescueModeThresholdMinutes",
                schema: "dsp",
                table: "DispatchConfigs");

            migrationBuilder.DropColumn(
                name: "SlaWeight",
                schema: "dsp",
                table: "DispatchConfigs");

            migrationBuilder.DropColumn(
                name: "UrgentRoundTimeoutSeconds",
                schema: "dsp",
                table: "DispatchConfigs");

            migrationBuilder.DropColumn(
                name: "ProjectedReadyAtUtc",
                schema: "dsp",
                table: "DispatchAttempts");

            migrationBuilder.DropColumn(
                name: "Strategy",
                schema: "dsp",
                table: "DispatchAttempts");
        }
    }
}
