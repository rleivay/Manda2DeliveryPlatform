using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverRejectionReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReasonId",
                schema: "dsp",
                table: "DispatchAttemptDrivers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonNotes",
                schema: "dsp",
                table: "DispatchAttemptDrivers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverRejectionReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RequiresNote = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverRejectionReasons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DriverRejectionReasons",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedByUserId", "DeletedAt", "DisplayName", "DisplayOrder", "IsActive", "IsDeleted", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, "TOO_FAR", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Muy lejos", 1, true, false, null, null },
                    { 2, "LOW_EARNINGS", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Ganancia baja", 2, true, false, null, null },
                    { 3, "UNSAFE_ZONE", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Zona insegura", 3, true, false, null, null },
                    { 4, "NO_BATTERY", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Sin batería", 4, true, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "DriverRejectionReasons",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedByUserId", "DeletedAt", "DisplayName", "DisplayOrder", "IsActive", "IsDeleted", "RequiresNote", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { 5, "OTHER", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Otro motivo", 5, true, false, true, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAttemptDrivers_ReasonId",
                schema: "dsp",
                table: "DispatchAttemptDrivers",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverRejectionReasons_Code",
                table: "DriverRejectionReasons",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchAttemptDrivers_DriverRejectionReasons_ReasonId",
                schema: "dsp",
                table: "DispatchAttemptDrivers",
                column: "ReasonId",
                principalTable: "DriverRejectionReasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchAttemptDrivers_DriverRejectionReasons_ReasonId",
                schema: "dsp",
                table: "DispatchAttemptDrivers");

            migrationBuilder.DropTable(
                name: "DriverRejectionReasons");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAttemptDrivers_ReasonId",
                schema: "dsp",
                table: "DispatchAttemptDrivers");

            migrationBuilder.DropColumn(
                name: "ReasonId",
                schema: "dsp",
                table: "DispatchAttemptDrivers");

            migrationBuilder.DropColumn(
                name: "ReasonNotes",
                schema: "dsp",
                table: "DispatchAttemptDrivers");
        }
    }
}
