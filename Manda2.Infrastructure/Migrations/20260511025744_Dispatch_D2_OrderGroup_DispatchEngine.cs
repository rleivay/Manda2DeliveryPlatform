using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Manda2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Dispatch_D2_OrderGroup_DispatchEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubOrders_Orders_OrderId",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.EnsureSchema(
                name: "dsp");

            migrationBuilder.EnsureSchema(
                name: "ord");

            migrationBuilder.RenameColumn(
                name: "CommerceId",
                schema: "core",
                table: "SubOrders",
                newName: "OrderGroupId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                schema: "core",
                table: "SubOrderDetails",
                newName: "MerchantProductId");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalCommissionAmount",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotal",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "core",
                table: "SubOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                schema: "core",
                table: "SubOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPayable",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeliveryFeeProrrated",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                schema: "core",
                table: "SubOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                schema: "core",
                table: "SubOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                schema: "core",
                table: "SubOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MerchantId",
                schema: "core",
                table: "SubOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                schema: "core",
                table: "SubOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyAt",
                schema: "core",
                table: "SubOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                schema: "core",
                table: "SubOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ResolvedCommissionPct",
                schema: "core",
                table: "SubOrders",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxPct",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalePrice",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "SAP_ItemCode",
                schema: "core",
                table: "SubOrderDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PurchasePrice",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotalWithTax",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotal",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                schema: "core",
                table: "SubOrderDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPct",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddColumn<decimal>(
                name: "NetSalePrice",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "core",
                table: "SubOrderDetails",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotImageUrl",
                schema: "core",
                table: "SubOrderDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitDiscount",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CurrentOrderGroupId",
                schema: "core",
                table: "Drivers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastLatitude",
                schema: "core",
                table: "Drivers",
                type: "decimal(18,10)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocationUpdateAt",
                schema: "core",
                table: "Drivers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastLongitude",
                schema: "core",
                table: "Drivers",
                type: "decimal(18,10)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSubOrderLimit",
                schema: "core",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DispatchConfigs",
                schema: "dsp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InitialRadiusKm = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RadiusExpansionFactor = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MaxDriversToNotifyPerRound = table.Column<int>(type: "int", nullable: false),
                    RoundTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxRounds = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DispatchConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderGroups",
                schema: "ord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubOrderCount = table.Column<int>(type: "int", nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryAddressText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DeliveryLatitude = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    DeliveryLongitude = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderGroups_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "core",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderGroups_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "core",
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DispatchAttempts",
                schema: "dsp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderGroupId = table.Column<int>(type: "int", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchAttempts_OrderGroups_OrderGroupId",
                        column: x => x.OrderGroupId,
                        principalSchema: "ord",
                        principalTable: "OrderGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderGroupStops",
                schema: "ord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderGroupId = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<int>(type: "int", nullable: true),
                    StopType = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AddressText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    EstimatedArrivalAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderGroupStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderGroupStops_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "core",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderGroupStops_OrderGroups_OrderGroupId",
                        column: x => x.OrderGroupId,
                        principalSchema: "ord",
                        principalTable: "OrderGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DispatchAttemptDrivers",
                schema: "dsp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchAttemptId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    Response = table.Column<int>(type: "int", nullable: false),
                    NotifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DistanceKm = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    EstimatedArrivalMinutes = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchAttemptDrivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchAttemptDrivers_DispatchAttempts_DispatchAttemptId",
                        column: x => x.DispatchAttemptId,
                        principalSchema: "dsp",
                        principalTable: "DispatchAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DispatchAttemptDrivers_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "core",
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Cantidad máxima de SubOrders (comercios) que puede llevar un repartidor por defecto", "DEFAULT_DRIVER_MAX_SUBORDER_LIMIT", "1" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "% de comisión para comercios tipo Marketplace");

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "% de comisión para comercios tipo DarkStore");

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "% de comisión para comercios tipo DarkKitchen (pendiente de definir)", "COMMISSION_DARKKITCHEN", "0.00" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Cargo fijo por servicio de plataforma", "SERVICE_FEE_FIXED", "50.00" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Cargo base por delivery (puede variar por zona)", "DELIVERY_FEE_BASE", "80.00" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Etiqueta configurable para indicar precio igual al local físico", "LABEL_SAME_PRICE_AS_STORE", "Mismo precio que en el local" });

            migrationBuilder.InsertData(
                schema: "cfg",
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "DeletedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "UpdatedByUserId", "Value" },
                values: new object[,]
                {
                    { 9, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Desviación máxima permitida en metros para agrupación de rutas", false, "MAX_DISTANCE_DEVIATION_MTS", null, null, "2000" },
                    { 10, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Radio inicial de búsqueda de repartidores en la Ronda 1", false, "DISPATCH_DEFAULT_INITIAL_RADIUS_KM", null, null, "3.00" },
                    { 11, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Kilómetros adicionales por cada ronda extra de dispatch", false, "DISPATCH_DEFAULT_RADIUS_EXPANSION_FACTOR", null, null, "2.00" },
                    { 12, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Número máximo de rondas de búsqueda antes de notificar al cliente", false, "DISPATCH_DEFAULT_MAX_ROUNDS", null, null, "3" },
                    { 13, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Minutos de espera por ronda antes de pasar a la siguiente", false, "DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES", null, null, "2" },
                    { 14, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Cantidad de repartidores notificados simultáneamente por ronda", false, "DISPATCH_DEFAULT_MAX_DRIVERS_PER_ROUND", null, null, "5" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubOrders_MerchantId",
                schema: "core",
                table: "SubOrders",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubOrders_OrderGroupId",
                schema: "core",
                table: "SubOrders",
                column: "OrderGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SubOrderDetails_MerchantProductId",
                schema: "core",
                table: "SubOrderDetails",
                column: "MerchantProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CurrentOrderGroupId",
                schema: "core",
                table: "Drivers",
                column: "CurrentOrderGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAttemptDrivers_DispatchAttemptId_DriverId",
                schema: "dsp",
                table: "DispatchAttemptDrivers",
                columns: new[] { "DispatchAttemptId", "DriverId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAttemptDrivers_DriverId",
                schema: "dsp",
                table: "DispatchAttemptDrivers",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAttempts_OrderGroupId_RoundNumber",
                schema: "dsp",
                table: "DispatchAttempts",
                columns: new[] { "OrderGroupId", "RoundNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchConfigs_ZoneName",
                schema: "dsp",
                table: "DispatchConfigs",
                column: "ZoneName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroups_CustomerId",
                schema: "ord",
                table: "OrderGroups",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroups_DriverId",
                schema: "ord",
                table: "OrderGroups",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroupStops_MerchantId",
                schema: "ord",
                table: "OrderGroupStops",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroupStops_OrderGroupId_Sequence",
                schema: "ord",
                table: "OrderGroupStops",
                columns: new[] { "OrderGroupId", "Sequence" });

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_OrderGroups_CurrentOrderGroupId",
                schema: "core",
                table: "Drivers",
                column: "CurrentOrderGroupId",
                principalSchema: "ord",
                principalTable: "OrderGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SubOrderDetails_MerchantProducts_MerchantProductId",
                schema: "core",
                table: "SubOrderDetails",
                column: "MerchantProductId",
                principalSchema: "core",
                principalTable: "MerchantProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubOrders_Merchants_MerchantId",
                schema: "core",
                table: "SubOrders",
                column: "MerchantId",
                principalSchema: "core",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubOrders_OrderGroups_OrderGroupId",
                schema: "core",
                table: "SubOrders",
                column: "OrderGroupId",
                principalSchema: "ord",
                principalTable: "OrderGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubOrders_Orders_OrderId",
                schema: "core",
                table: "SubOrders",
                column: "OrderId",
                principalSchema: "core",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_OrderGroups_CurrentOrderGroupId",
                schema: "core",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_SubOrderDetails_MerchantProducts_MerchantProductId",
                schema: "core",
                table: "SubOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubOrders_Merchants_MerchantId",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SubOrders_OrderGroups_OrderGroupId",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SubOrders_Orders_OrderId",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropTable(
                name: "DispatchAttemptDrivers",
                schema: "dsp");

            migrationBuilder.DropTable(
                name: "DispatchConfigs",
                schema: "dsp");

            migrationBuilder.DropTable(
                name: "OrderGroupStops",
                schema: "ord");

            migrationBuilder.DropTable(
                name: "DispatchAttempts",
                schema: "dsp");

            migrationBuilder.DropTable(
                name: "OrderGroups",
                schema: "ord");

            migrationBuilder.DropIndex(
                name: "IX_SubOrders_MerchantId",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropIndex(
                name: "IX_SubOrders_OrderGroupId",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropIndex(
                name: "IX_SubOrderDetails_MerchantProductId",
                schema: "core",
                table: "SubOrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CurrentOrderGroupId",
                schema: "core",
                table: "Drivers");

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "ReadyAt",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "ResolvedCommissionPct",
                schema: "core",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "NetSalePrice",
                schema: "core",
                table: "SubOrderDetails");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "core",
                table: "SubOrderDetails");

            migrationBuilder.DropColumn(
                name: "SnapshotImageUrl",
                schema: "core",
                table: "SubOrderDetails");

            migrationBuilder.DropColumn(
                name: "UnitDiscount",
                schema: "core",
                table: "SubOrderDetails");

            migrationBuilder.DropColumn(
                name: "CurrentOrderGroupId",
                schema: "core",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LastLatitude",
                schema: "core",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LastLocationUpdateAt",
                schema: "core",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LastLongitude",
                schema: "core",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "MaxSubOrderLimit",
                schema: "core",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "OrderGroupId",
                schema: "core",
                table: "SubOrders",
                newName: "CommerceId");

            migrationBuilder.RenameColumn(
                name: "MerchantProductId",
                schema: "core",
                table: "SubOrderDetails",
                newName: "ItemId");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalCommissionAmount",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotal",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "core",
                table: "SubOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                schema: "core",
                table: "SubOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPayable",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeliveryFeeProrrated",
                schema: "core",
                table: "SubOrders",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxPct",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalePrice",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "SAP_ItemCode",
                schema: "core",
                table: "SubOrderDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PurchasePrice",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotalWithTax",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotal",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                schema: "core",
                table: "SubOrderDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPct",
                schema: "core",
                table: "SubOrderDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Grupos máximos por defecto para nuevos repartidores", "DEFAULT_DRIVER_MAX_ACTIVE_GROUPS", "3" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "% de comisión para Marketplace");

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "% de comisión para DarkStore");

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Cargo fijo por servicio", "SERVICE_FEE_FIXED", "50.00" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Cargo base por delivery", "DELIVERY_FEE_BASE", "80.00" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Etiqueta configurable para precios iguales", "LABEL_SAME_PRICE_AS_STORE", "Mismo precio que en el local" });

            migrationBuilder.UpdateData(
                schema: "cfg",
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Key", "Value" },
                values: new object[] { "Desviación máxima permitida en metros para agrupación de rutas", "MAX_DISTANCE_DEVIATION_MTS", "2000" });

            migrationBuilder.AddForeignKey(
                name: "FK_SubOrders_Orders_OrderId",
                schema: "core",
                table: "SubOrders",
                column: "OrderId",
                principalSchema: "core",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
