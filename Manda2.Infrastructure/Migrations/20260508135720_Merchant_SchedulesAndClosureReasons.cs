using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Merchant_SchedulesAndClosureReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClosureReasons",
                schema: "cfg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosureReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MerchantScheduleExceptions",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantId = table.Column<int>(type: "int", nullable: false),
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
                    MerchantId = table.Column<int>(type: "int", nullable: false),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantScheduleExceptions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "MerchantSchedules",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ClosureReasons",
                schema: "cfg");
        }
    }
}
