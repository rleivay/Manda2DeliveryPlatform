using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecuritySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sec");

            migrationBuilder.CreateTable(
                name: "AppUsers",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.CheckConstraint("CK_AppUser_SingleActor", "(CASE WHEN CustomerId IS NOT NULL THEN 1 ELSE 0 END +\r\n                           CASE WHEN DriverId   IS NOT NULL THEN 1 ELSE 0 END +\r\n                           CASE WHEN MerchantId IS NOT NULL THEN 1 ELSE 0 END) <= 1");
                });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "DeletedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "UpdatedByUserId", "Value" },
                values: new object[,]
                {
                    { 15, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Frecuencia con la que la App del Driver debe reportar GPS (al estar en ruta)", false, "DRIVER_GPS_UPDATE_INTERVAL_SECONDS", null, null, "30" },
                    { 16, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Distancia mínima recorrida para forzar una actualización de GPS antes del intervalo", false, "DRIVER_GPS_MIN_DISTANCE_METERS", null, null, "50" },
                    { 17, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Minutos tras los cuales una ubicación se considera obsoleta para el motor de Dispatch", false, "DRIVER_STALE_LOCATION_MINUTES", null, null, "10" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Email",
                schema: "sec",
                table: "AppUsers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUsers",
                schema: "sec");

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 17);
        }
    }
}
