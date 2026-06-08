using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "cfg",
                table: "ClosureReasons",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "DeletedAt", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Cierre por día feriado nacional o local", true, false, "Día feriado", null, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Cierre temporal por mantenimiento del local o vehículo", true, false, "Mantenimiento", null, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Comercio sin stock suficiente para operar", true, false, "Falta de insumos", null, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Cierre de emergencia no planificado", true, false, "Emergencia", null, null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Cierre programado por período vacacional", true, false, "Vacaciones", null, null },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Razón no categorizada — requiere nota manual", true, false, "Otro", null, null }
                });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "CreditNoteTypes",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "DeletedAt", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "Devolución por pedido cancelado", null, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "Compensación por error de entrega", null, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "GiftCard / Promoción", null, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "Ajuste manual BackOffice", null, null }
                });

            migrationBuilder.InsertData(
                schema: "dsp",
                table: "DispatchConfigs",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "DeletedAt", "InitialRadiusKm", "IsActive", "IsDeleted", "MaxDriversToNotifyPerRound", "MaxRounds", "RadiusExpansionFactor", "RoundTimeoutMinutes", "UpdatedAt", "UpdatedByUserId", "ZoneName" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3.00m, true, false, 5, 3, 2.00m, 2, null, null, "Nicaragua" });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "PaymentMethods",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedByUserId", "DeletedAt", "IsActive", "IsDeleted", "Name", "RequiresReference", "RequiresVoucher", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, "CASH", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "Efectivo", false, false, null, null },
                    { 2, "CARD", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "Tarjeta", true, false, null, null },
                    { 3, "TRANSFER", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "Transferencia", true, true, null, null },
                    { 4, "CREDIT_NOTE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, "Nota de Crédito", true, false, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "ClosureReasons",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "ClosureReasons",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "ClosureReasons",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "ClosureReasons",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "ClosureReasons",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "ClosureReasons",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "CreditNoteTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "CreditNoteTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "CreditNoteTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "CreditNoteTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dsp",
                table: "DispatchConfigs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "PaymentMethods",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
