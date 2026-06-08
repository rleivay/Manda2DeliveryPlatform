
using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Manda2.Contracts.CheckOut;

namespace Manda2.Application.Feature.OrderGroups.Commands
{
    /// <summary>
    /// Crea un OrderGroup multi-comercio (Draft) a partir del carrito recibido.
    /// Reglas aplicadas:
    ///  - Valida límite MAX SUBORDERS desde AppConfig (DEFAULT_DRIVER_MAX_SUBORDER_LIMIT).
    ///  - Carga MerchantProducts y Merchants en batch.
    ///  - Valida que cada MerchantProduct pertenezca al Merchant indicado y esté disponible.
    ///  - Si alguna suborden queda vacía => FAIL (según petición H).
    ///  - Snapshot de precios en SubOrderDetail.
    ///  - Crea Stops de Pickup con la ubicación actual del Merchant (auditabilidad).
    ///  - Crea Stop Dropoff final con DeliveryAddress/Lat/Lon del comando.
    ///  - Lee fees desde AppConfig (SERVICE_FEE_FIXED, DELIVERY_FEE_BASE).
    ///  - Inserta AuditLog.
    /// </summary>
    public class CreateOrderGroupCommandHandler
        : ICommandHandler<CreateOrderGroupCommand, CreateOrderGroupResult>
    {
        private readonly IApplicationDbContext _db;

        public CreateOrderGroupCommandHandler(IApplicationDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<CreateOrderGroupResult> HandleAsync(
            CreateOrderGroupCommand command, CancellationToken ct = default)
        {
            try {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (command.Merchants == null || !command.Merchants.Any())
                return CreateOrderGroupResult.Failure("El carrito está vacío.");

            // 1) Leer límite global de SubOrders desde AppConfig
            var maxSubOrders = await GetConfigIntAsync("DEFAULT_DRIVER_MAX_SUBORDER_LIMIT", 1, ct);
            if (command.Merchants.Count > maxSubOrders)
            {
                return CreateOrderGroupResult.Failure(
                    $"El carrito contiene {command.Merchants.Count} comercios, " +
                    $"pero el límite configurado es {maxSubOrders}.");
            }

            // 2) Recolectar IDs de MerchantProduct y MerchantIds
            var allMerchantProductIds = command.Merchants
                .SelectMany(m => m.Items)
                .Select(i => i.ProductId) // NOTE: CartItemDto: ProductId (pero en dominio usamos MerchantProduct.Id)
                .Distinct()
                .ToList();

            // Dado que el DTO CartItemDto tiene ProductId, necesitamos resolverlo contra MerchantProducts.
            // Buscamos MerchantProducts por ProductId y MerchantId conjunto para evitar colisiones.
            // Construimos lista de (merchantId, productId) pairs
            var merchantProductPairs = command.Merchants
                .SelectMany(m => m.Items.Select(i => new { m.MerchantId, ProductId = i.ProductId }))
                .Distinct()
                .ToList();

            // Cargamos MerchantProducts filtrando por los pares MerchantId+ProductId
            var merchantProductsQuery = _db.MerchantProducts
                .Include(mp => mp.Product)
                .AsQueryable();

            // Filtrar por pares (eficiente: cargar por MerchantId en la lista)
            var merchantIds = merchantProductPairs.Select(p => p.MerchantId).Distinct().ToList();
            merchantProductsQuery = merchantProductsQuery.Where(mp => merchantIds.Contains(mp.MerchantId));

            var merchantProductsList = await merchantProductsQuery.ToListAsync(ct);

            // Creamos un lookup por (MerchantId, ProductId) -> MerchantProduct
            var merchantProductLookup = merchantProductsList
                .GroupBy(mp => (mp.MerchantId, mp.ProductId))
                .ToDictionary(g => g.Key, g => g.First());

            // 3) Cargar Merchants para poblar Stops
            var merchantsToLoad = command.Merchants.Select(m => m.MerchantId).Distinct().ToList();
            var merchantsMap = await _db.Merchants
                .Where(m => merchantsToLoad.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, ct);

            // 4) Construir OrderGroup
            var group = new OrderGroup
            {
                CustomerId = command.CustomerId,
                Status = OrderGroupStatus.Draft,
                DeliveryAddressText = command.DeliveryAddress ?? string.Empty,
                DeliveryLatitude = command.Lat,
                DeliveryLongitude = command.Lon,
                SubOrderCount = command.Merchants.Count,
                ServiceFee = 0m,
                DeliveryFee = 0m,
                TotalAmount = 0m
            };

            decimal totalGlobal = 0m;
            int stopSequence = 1;

            // 5) Procesar cada MerchantCartDto
            foreach (var merchantDto in command.Merchants)
            {
                var subOrder = new SubOrder
                {
                    MerchantId = merchantDto.MerchantId,
                    Status = SubOrderStatus.PendingMerchantAcceptance,
                    SubTotal = 0m,
                    ResolvedCommissionPct = 0m,
                    TotalCommissionAmount = 0m,
                    DeliveryFeeProrrated = 0m,
                    NetPayable = 0m
                };

                foreach (var item in merchantDto.Items)
                {
                    // Buscar MerchantProduct por (MerchantId, ProductId)
                    if (!merchantProductLookup.TryGetValue((merchantDto.MerchantId, item.ProductId),
                        out var mp))
                    {
                        // Producto no encontrado para el comercio: error según regla H
                        return CreateOrderGroupResult.Failure(
                            $"Producto (ProductId={item.ProductId}) no encontrado en el comercio {merchantDto.MerchantId}.");
                    }

                    if (!mp.IsAvailable)
                    {
                        return CreateOrderGroupResult.Failure(
                            $"El producto '{mp.Product?.Name ?? mp.Id.ToString()}' no está disponible en el comercio {merchantDto.MerchantId}.");
                    }

                    // Snapshot precios
                    var netSalePrice = mp.SalePrice; // UnitDiscount = 0 en MVP
                    var lineTotal = netSalePrice * item.Quantity;

                    var detail = new SubOrderDetail
                    {
                        MerchantProductId = mp.Id,
                        Quantity = item.Quantity,

                        // Snapshot
                        ItemName = mp.Product?.Name ?? string.Empty,
                        SnapshotImageUrl = mp.Product?.ImageUrl,
                        PurchasePrice = mp.BasePrice,
                        SalePrice = mp.SalePrice,
                        UnitDiscount = 0m,
                        NetSalePrice = netSalePrice,
                        CommissionPct = mp.ResolvedCommissionPct ?? 0m,
                        TaxPct = 0m,
                        TaxAmount = 0m,
                        LineTotal = lineTotal,
                        LineTotalWithTax = lineTotal,
                        Notes = item.SpecialInstructions,
                        SAP_ItemCode = null
                    };

                    subOrder.Details.Add(detail);
                    subOrder.SubTotal += detail.LineTotal;
                }

                // Si por alguna razón subOrder quedó sin líneas (no debería ocurrir por validaciones previas), error
                if (!subOrder.Details.Any())
                {
                    return CreateOrderGroupResult.Failure(
                        $"La suborden para el comercio {merchantDto.MerchantId} no contiene líneas válidas.");
                }

                // Determinar ResolvedCommissionPct (tomamos del primer detalle como aproximación en MVP)
                subOrder.ResolvedCommissionPct = subOrder.Details.First().CommissionPct;
                subOrder.TotalCommissionAmount = Math.Round(subOrder.SubTotal * (subOrder.ResolvedCommissionPct / 100m), 2);

                group.SubOrders.Add(subOrder);
                totalGlobal += subOrder.SubTotal;

                // Crear Stop Pickup con la ubicación del Merchant (auditabilidad)
                if (!merchantsMap.TryGetValue(merchantDto.MerchantId, out var merchantData))
                {
                    return CreateOrderGroupResult.Failure($"Comercio {merchantDto.MerchantId} no encontrado.");
                }

                group.Stops.Add(new OrderGroupStop
                {
                    MerchantId = merchantDto.MerchantId,
                    StopType = DispatchEnums.StopType.Pickup,
                    Sequence = stopSequence++,
                    AddressText = merchantData.AddressText,
                    Latitude = merchantData.Latitude,
                    Longitude = merchantData.Longitude,
                    IsCompleted = false
                });
            }

            // 6) Stop Dropoff final (cliente)
            group.Stops.Add(new OrderGroupStop
            {
                MerchantId = null,
                StopType = DispatchEnums.StopType.Dropoff,
                Sequence = stopSequence,
                AddressText = command.DeliveryAddress ?? string.Empty,
                Latitude = command.Lat,
                Longitude = command.Lon,
                IsCompleted = false
            });

            // 7) Leer FEES desde AppConfig
            var serviceFee = await GetConfigDecimalAsync("SERVICE_FEE_FIXED", 0m, ct);
            var deliveryFee = await GetConfigDecimalAsync("DELIVERY_FEE_BASE", 0m, ct);

            group.ServiceFee = serviceFee;
            group.DeliveryFee = deliveryFee;
            group.TotalAmount = Math.Round(totalGlobal + serviceFee + deliveryFee, 2);

            // 8) Persistir: OrderGroup + AuditLog
            _db.OrderGroups.Add(group);

            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroup),
                EntityId = 0, // se actualizará tras SaveChanges
                Action = "CreateOrderGroup",
                PerformedByUserId = command.CustomerId,
                Details = $"Draft creado. Comercios: {group.SubOrders.Count}. Total: {group.TotalAmount:F2}"
            });

            await _db.SaveChangesAsync(ct);

            // Actualizar AuditLog local con el Id asignado (opcional)
            var audit = _db.AuditLogs.Local.LastOrDefault(a => a.Action == "CreateOrderGroup"
                && a.PerformedByUserId == command.CustomerId);
            if (audit != null)
            {
                audit.EntityId = group.Id;
                await _db.SaveChangesAsync(ct);
            }

            // 9) Retornar resultado con resumen financiero (según J)
            return CreateOrderGroupResult.Success(
                group.Id,
                group.TotalAmount,
                group.ServiceFee,
                group.DeliveryFee,
                group.SubOrders.Count);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #region Helpers: Leer AppConfig

        private async Task<int> GetConfigIntAsync(string key, int defaultValue, CancellationToken ct)
        {
            var cfg = await _db.AppConfigs.FirstOrDefaultAsync(c => c.Key == key, ct);
            if (cfg == null) return defaultValue;
            if (int.TryParse(cfg.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;
            return defaultValue;
        }

        private async Task<decimal> GetConfigDecimalAsync(string key, decimal defaultValue, CancellationToken ct)
        {
            var cfg = await _db.AppConfigs.FirstOrDefaultAsync(c => c.Key == key, ct);
            if (cfg == null) return defaultValue;
            if (decimal.TryParse(cfg.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;
            return defaultValue;
        }

        #endregion
    }

}
