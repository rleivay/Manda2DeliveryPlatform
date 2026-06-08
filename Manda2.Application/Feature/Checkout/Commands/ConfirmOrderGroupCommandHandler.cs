// PROPÓSITO:
//   Handler de consolidación logística del checkout.
//   Calcula el DeliveryFee real usando Haversine y proratea entre SubOrders.
//   Snapshot de dirección en OrderGroup y Stop Dropoff.
//   Transiciona Draft → CapacityValidated.
//
// DEPENDENCIAS: IApplicationDbContext
// PATRÓN: ICommandHandler<TCommand, TResult> — mediador propio Manda2.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;
using Manda2.Contracts.Enum;
using Manda2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Manda2.Application.Feature.Checkout.Commands
{
    /// <summary>
    /// Handler del comando ConfirmOrderGroupCommand.
    /// Consolida dirección real y recalcula delivery fee antes del pago.
    /// </summary>
    public class ConfirmOrderGroupCommandHandler
        : ICommandHandler<ConfirmOrderGroupCommand, ConfirmOrderGroupResult>
    {
        private readonly IApplicationDbContext _db;

        public ConfirmOrderGroupCommandHandler(IApplicationDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<ConfirmOrderGroupResult> HandleAsync(
            ConfirmOrderGroupCommand request,
            CancellationToken ct)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Cargar OrderGroup con SubOrders y Stops ────
            var group = await _db.OrderGroups
                .Include(og => og.SubOrders)
                    .ThenInclude(so => so.Merchant)
                .Include(og => og.Stops)
                .FirstOrDefaultAsync(og => og.Id == request.OrderGroupId, ct);

            if (group == null)
                throw new InvalidOperationException(
                    $"OrderGroup {request.OrderGroupId} no encontrado.");

            // ── PASO 2: Validar ownership del OrderGroup ────
            if (group.CustomerId != request.CustomerId)
                throw new UnauthorizedAccessException(
                    $"El cliente {request.CustomerId} no es propietario del OrderGroup {request.OrderGroupId}.");

            // ── PASO 3: Validar estado — solo Draft puede consolidarse ────
            if (group.Status != OrderGroupStatus.Draft)
                throw new InvalidOperationException(
                    $"El OrderGroup {request.OrderGroupId} no está en estado Draft. " +
                    $"Estado actual: {group.Status}.");

            // ── PASO 4: Cargar y validar ShippingAddress ────
            var address = await _db.ShippingAddresses
                .FirstOrDefaultAsync(sa =>
                    sa.Id == request.ShippingAddressId
                    && sa.CustomerId == request.CustomerId
                    && sa.IsActive
                    && !sa.IsDeleted, ct);

            if (address == null)
                throw new InvalidOperationException(
                    $"ShippingAddress {request.ShippingAddressId} no encontrada o no pertenece al cliente {request.CustomerId}.");

            // ── PASO 5: Leer configuración de delivery fee desde AppConfig ────
            var feeBase = await GetConfigDecimalAsync("DELIVERY_FEE_BASE", 0m, ct);
            var freeKm = await GetConfigDecimalAsync("DELIVERY_FEE_FREE_KM", 4m, ct);
            var feePerKm = await GetConfigDecimalAsync("DELIVERY_FEE_PER_KM", 0m, ct);

            // ── PASO 6: Calcular distancia Haversine por SubOrder ────
            // Merchant.Latitude/Longitude son decimal → castear a double para Haversine.
            // ShippingAddress.Latitude/Longitude son double.
            var subOrders = group.SubOrders.ToList();

            var distances = subOrders.Select(so => new
            {
                SubOrder = so,
                DistanceKm = HaversineKm(
                    (double)so.Merchant.Latitude,
                    (double)so.Merchant.Longitude,
                    address.Latitude,
                    address.Longitude)
            }).ToList();

            // ── PASO 7: Calcular fee por SubOrder y fee total del grupo ────
            // Regla: 0-freeKm → solo feeBase; >freeKm → base + (excedente * feePerKm)
            var feesPerSubOrder = distances.Select(d => new
            {
                d.SubOrder,
                d.DistanceKm,
                Fee = CalculateFee(d.DistanceKm, (double)feeBase, (double)freeKm, (double)feePerKm)
            }).ToList();

            decimal totalGroupFee = (decimal)feesPerSubOrder.Sum(f => f.Fee);
            double totalGroupDistance = feesPerSubOrder.Sum(f => f.DistanceKm);

            // ── PASO 8: Prorratear fee y actualizar SubOrders ────
            foreach (var item in feesPerSubOrder)
            {
                var pct = totalGroupDistance > 0
                    ? item.DistanceKm / totalGroupDistance * 100.0
                    : (100.0 / subOrders.Count); // distribución equitativa si distancias = 0

                var prorrated = totalGroupDistance > 0
                    ? (decimal)(item.Fee / feesPerSubOrder.Sum(f => f.Fee) * (double)totalGroupFee)
                    : totalGroupFee / subOrders.Count;

                item.SubOrder.DeliveryFeeProrrated = Math.Round(prorrated, 2);
                item.SubOrder.DeliveryDistanceKm = Math.Round((decimal)item.DistanceKm, 4);
                item.SubOrder.DeliveryDistancePct = Math.Round((decimal)pct, 4);
                item.SubOrder.SnapshotFeeBase = feeBase;
                item.SubOrder.SnapshotFeeKmIncluidos = freeKm;
                item.SubOrder.SnapshotFeePorKm = feePerKm;
                item.SubOrder.SnapshotTotalGroupDistanceKm = Math.Round((decimal)totalGroupDistance, 4);
                item.SubOrder.SnapshotTotalGroupFee = totalGroupFee;

                // Recalcular NetPayable con el fee real
                item.SubOrder.NetPayable = Math.Round(
                    item.SubOrder.SubTotal
                    - item.SubOrder.TotalCommissionAmount
                    - item.SubOrder.DeliveryFeeProrrated, 2);
            }

            // ── PASO 9: Snapshot de dirección en OrderGroup ────
            var addressText = BuildAddressText(address);

            group.DeliveryAddressText = addressText;
            group.DeliveryLatitude = (decimal)address.Latitude;
            group.DeliveryLongitude = (decimal)address.Longitude;
            group.DeliveryFee = totalGroupFee;

            // Recalcular TotalAmount con el delivery fee real
            decimal subTotal = subOrders.Sum(so => so.SubTotal);
            group.TotalAmount = Math.Round(subTotal + group.ServiceFee + totalGroupFee, 2);

            // ── PASO 10: Actualizar Stop Dropoff con la dirección real ────
            var dropoff = group.Stops
                .FirstOrDefault(s => s.StopType == DispatchEnums.StopType.Dropoff);

            if (dropoff != null)
            {
                dropoff.AddressText = addressText;
                dropoff.Latitude = (decimal)address.Latitude;
                dropoff.Longitude = (decimal)address.Longitude;
            }

            // ── PASO 11: Transición de estado ────
            group.Status = OrderGroupStatus.CapacityValidated;
            group.UpdatedAt = utcNow;

            // ── PASO 12: AuditLog ────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroup),
                EntityId = group.Id,
                Action = "ConfirmOrderGroup",
                PerformedByUserId = request.CustomerId,
                Details = $"ShippingAddressId: {request.ShippingAddressId} | " +
                             $"Dirección: {addressText} | " +
                             $"DeliveryFee: {totalGroupFee:F2} | " +
                             $"TotalAmount: {group.TotalAmount:F2} | " +
                             $"SubOrders: {subOrders.Count}",
                Category = "CHECKOUT",
                CreatedAt = utcNow
            });

            // ── PASO 13: Persistir ────
            await _db.SaveChangesAsync(ct);

            return ConfirmOrderGroupResult.Success(
                orderGroupId: group.Id,
                subTotal: subTotal,
                deliveryFee: totalGroupFee,
                serviceFee: group.ServiceFee,
                totalAmount: group.TotalAmount,
                addressText: addressText);
        }

        // ════════════════════════════════════════════════════════════════════
        // HELPERS PRIVADOS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Fórmula de Haversine. Retorna distancia en kilómetros.
        /// </summary>
        private static double HaversineKm(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371.0; // Radio de la Tierra en km

            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;

        /// <summary>
        /// Calcula el fee de delivery para una distancia dada.
        /// Regla: 0-freeKm → solo feeBase; >freeKm → base + (excedente * feePerKm).
        /// </summary>
        private static double CalculateFee(
            double distanceKm,
            double feeBase,
            double freeKm,
            double feePerKm)
        {
            if (distanceKm <= freeKm)
                return feeBase;

            return feeBase + (distanceKm - freeKm) * feePerKm;
        }

        /// <summary>
        /// Construye el texto de dirección para snapshot.
        /// Formato: "AddressLine1, AddressLine2, City" (omite AddressLine2 si es null).
        /// </summary>
        private static string BuildAddressText(ShippingAddress address)
        {
            var parts = new List<string> { address.AddressLine1 };

            if (!string.IsNullOrWhiteSpace(address.AddressLine2))
                parts.Add(address.AddressLine2);

            parts.Add(address.City);

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Lee un valor decimal desde cfg.AppConfigs.
        /// Retorna defaultValue si la clave no existe o no es parseable.
        /// </summary>
        private async Task<decimal> GetConfigDecimalAsync(
            string key, decimal defaultValue, CancellationToken ct)
        {
            var cfg = await _db.AppConfigs
                .FirstOrDefaultAsync(c => c.Key == key && !c.IsDeleted, ct);

            if (cfg == null) return defaultValue;

            return decimal.TryParse(
                cfg.Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var v) ? v : defaultValue;
        }
    }
}