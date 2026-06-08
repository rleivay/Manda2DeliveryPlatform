using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Unified_OperatingSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantScheduleExceptions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "MerchantSchedules",
                schema: "core");

            migrationBuilder.RenameColumn(
                name: "IsOpen",
                schema: "core",
                table: "Merchants",
                newName: "IsOnline");

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                schema: "core",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OperatingScheduleExceptions",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorType = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ClosureReasonId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingScheduleExceptions", x => x.Id);
                    table.CheckConstraint("CK_OperatingScheduleException_Actor", "(MerchantId IS NOT NULL AND DriverId IS NULL) OR (MerchantId IS NULL AND DriverId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_OperatingScheduleExceptions_ClosureReasons_ClosureReasonId",
                        column: x => x.ClosureReasonId,
                        principalSchema: "cfg",
                        principalTable: "ClosureReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatingScheduleExceptions_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "core",
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatingScheduleExceptions_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "core",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatingSchedules",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorType = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ClosureReasonId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingSchedules", x => x.Id);
                    table.CheckConstraint("CK_OperatingSchedule_Actor", "(MerchantId IS NOT NULL AND DriverId IS NULL) OR (MerchantId IS NULL AND DriverId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_OperatingSchedules_ClosureReasons_ClosureReasonId",
                        column: x => x.ClosureReasonId,
                        principalSchema: "cfg",
                        principalTable: "ClosureReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatingSchedules_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "core",
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatingSchedules_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "core",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatingScheduleExceptions_ClosureReasonId",
                schema: "core",
                table: "OperatingScheduleExceptions",
                column: "ClosureReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingScheduleExceptions_DriverId",
                schema: "core",
                table: "OperatingScheduleExceptions",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingScheduleExceptions_MerchantId",
                schema: "core",
                table: "OperatingScheduleExceptions",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingSchedules_ClosureReasonId",
                schema: "core",
                table: "OperatingSchedules",
                column: "ClosureReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingSchedules_DriverId",
                schema: "core",
                table: "OperatingSchedules",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingSchedules_MerchantId",
                schema: "core",
                table: "OperatingSchedules",
                column: "MerchantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatingScheduleExceptions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "OperatingSchedules",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                schema: "core",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "IsOnline",
                schema: "core",
                table: "Merchants",
                newName: "IsOpen");

            migrationBuilder.CreateTable(
                name: "MerchantScheduleExceptions",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClosureReasonId = table.Column<int>(type: "int", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantScheduleExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantScheduleExceptions_ClosureReasons_ClosureReasonId",
                        column: x => x.ClosureReasonId,
                        principalSchema: "cfg",
                        principalTable: "ClosureReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MerchantScheduleExceptions_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "core",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MerchantSchedules",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClosureReasonId = table.Column<int>(type: "int", nullable: true),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantSchedules_ClosureReasons_ClosureReasonId",
                        column: x => x.ClosureReasonId,
                        principalSchema: "cfg",
                        principalTable: "ClosureReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MerchantSchedules_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "core",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantScheduleExceptions_ClosureReasonId",
                schema: "core",
                table: "MerchantScheduleExceptions",
                column: "ClosureReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantScheduleExceptions_MerchantId",
                schema: "core",
                table: "MerchantScheduleExceptions",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantSchedules_ClosureReasonId",
                schema: "core",
                table: "MerchantSchedules",
                column: "ClosureReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantSchedules_MerchantId",
                schema: "core",
                table: "MerchantSchedules",
                column: "MerchantId");
        }
    }
}
