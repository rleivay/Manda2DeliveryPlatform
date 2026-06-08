// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Queries/GetPendingOffers/
//          GetPendingOffersHandler.cs
//
// PROPÓSITO: Recupera y transforma las ofertas activas para un driver.
//
// FLUJO:
//   1. Busca en DispatchAttemptDrivers donde Response = Pending.
//   2. El DispatchAttempt debe estar con Status = Sent (Activo).
//   3. El OrderGroup debe estar en Status = AwaitingDriverAssignment.
//   4. Calcula distancias geográficas con fórmula Haversine.
//   5. Calcula el tiempo restante antes de que la oferta expire.
//   6. Usa PaymentMethodName desnormalizado — sin joins a Payments.
//
// CORRECCIÓN v2:
//   - Eliminado Include de og.Payments (innecesario, campo desnormalizado).
//   - PaymentMethod se lee de og.PaymentMethodName directamente.
//   - Eliminado cast (int) en comparación de enums (EF Core lo resuelve).
//
// NOTA ARQUITECTURAL:
//   Usa .AsNoTracking() para máxima performance ya que es solo lectura.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using static Manda2.Contracts.Enum.DispatchEnums;
using Manda2.Contracts.Enum;

namespace Manda2.Application.Feature.Driver.Queries
{
    /// <summary>
    /// Handler de GetPendingOffersQuery.
    /// Retorna las ofertas activas del driver ordenadas por distancia al primer stop.
    /// </summary>
    public class GetPendingOffersHandler
        : IQueryHandler<GetPendingOffersQuery, List<PendingOfferDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetPendingOffersHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PendingOfferDto>> HandleAsync(
            GetPendingOffersQuery query, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // ─────────────────────────────────────────────────────────────
            // PASO 1: Cargar notificaciones pendientes del driver.
            //
            // Includes necesarios:
            //   - DispatchAttempt → para Status y OrderGroupId
            //   - OrderGroup      → para datos financieros y de entrega
            //   - SubOrders + Merchant → para MerchantNames
            //   - Stops           → para calcular distancia al primer stop
            //
            // NOTA: NO se incluye og.Payments porque PaymentMethodName
            //       está desnormalizado en OrderGroup. Evita un join extra.
            // ─────────────────────────────────────────────────────────────
            var notifications = await _db.DispatchAttemptDrivers
                .AsNoTracking()
                .Include(nd => nd.DispatchAttempt)
                    .ThenInclude(da => da.OrderGroup)
                        .ThenInclude(og => og.SubOrders)
                            .ThenInclude(s => s.Merchant)
                .Include(nd => nd.DispatchAttempt)
                    .ThenInclude(da => da.OrderGroup)
                        .ThenInclude(og => og.Stops)
                .Where(nd =>
                    nd.DriverId == query.DriverId &&
                    nd.Response == DriverDispatchResponse.Pending &&
                    nd.DispatchAttempt.Status == DispatchAttemptStatus.Sent &&
                    nd.DispatchAttempt.OrderGroup.Status == OrderGroupStatus.AwaitingDriverAssignment)
                .ToListAsync(ct);

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Transformar a DTO con lógica de negocio en memoria.
            //
            // El timeout de oferta (2 min) es un valor que en Sprint 4
            // debe venir de cfg.AppConfigs (clave: "DispatchOfferTimeoutMinutes").
            // Por ahora se deja como constante documentada.
            // ─────────────────────────────────────────────────────────────
            const int offerTimeoutMinutes = 2; // TODO Sprint 4: leer de cfg.AppConfigs

            var result = new List<PendingOfferDto>();

            foreach (var nd in notifications)
            {
                var og = nd.DispatchAttempt?.OrderGroup;
                if (og == null) continue;

                // ─── Calcular tiempo restante ──────────────────────────
                var expiresAt = nd.CreatedAt.AddMinutes(offerTimeoutMinutes);
                var secondsRemaining = (int)(expiresAt - now).TotalSeconds;

                // Filtrar ofertas ya expiradas en memoria.
                // El background job de Sprint 4 las marcará como Expired en BD.
                if (secondsRemaining <= 0) continue;

                // ─── Primer stop (Pickup) para distancia ───────────────
                // Sequence 1 = primer comercio a visitar.
                var firstStop = og.Stops
                    .OrderBy(s => s.Sequence)
                    .FirstOrDefault();

                // ─── Nombres de comercios (sin duplicados) ─────────────
                var merchantNames = og.SubOrders
                    .Select(s => s.Merchant?.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!)
                    .Distinct()
                    .ToList();

                result.Add(new PendingOfferDto
                {
                    OrderGroupId = og.Id,

                    // ─── Financiero ────────────────────────────────────
                    // DeliveryFee: lo que gana el driver por esta ruta.
                    DriverEarnings = og.DeliveryFee,

                    // PaymentMethodName desnormalizado — sin join a Payments.
                    // CORRECCIÓN: antes se usaba og.Payments.FirstOrDefault()
                    // lo cual requería un Include adicional y podía retornar
                    // null si Payments no estaba cargado.
                    PaymentMethod = og.PaymentMethodName ?? "Efectivo",

                    // TotalAmount: lo que el driver debe cobrar si es Efectivo.
                    // La app MAUI debe mostrar este campo solo cuando
                    // PaymentMethod == "Efectivo" o "CASH".
                    AmountToCollect = og.TotalAmount,

                    // ─── Logístico ─────────────────────────────────────
                    SubOrderCount = og.SubOrders.Count,
                    TotalStops = og.Stops.Count,

                    // ─── Proximidad ────────────────────────────────────
                    DistanceToFirstStopKm = firstStop != null
                        ? CalculateDistance(
                            (double)query.CurrentLatitude,
                            (double)query.CurrentLongitude,
                            (double)firstStop.Latitude,
                            (double)firstStop.Longitude)
                        : 0,

                    // ─── Tiempo ────────────────────────────────────────
                    OfferedAtUtc = nd.CreatedAt,
                    SecondsRemaining = secondsRemaining,

                    // ─── Comercios ─────────────────────────────────────
                    MerchantNames = merchantNames
                });
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Ordenar por distancia al primer stop.
            // El driver ve primero las ofertas más cercanas.
            // ─────────────────────────────────────────────────────────────
            return result
                .OrderBy(x => x.DistanceToFirstStopKm)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────────
        // Fórmula de Haversine para distancia entre dos coordenadas GPS.
        // Retorna kilómetros. Precisión suficiente para rangos de dispatch
        // (< 20 km). Para distancias mayores usar Vincenty.
        // ─────────────────────────────────────────────────────────────────
        private static double CalculateDistance(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double earthRadiusKm = 6371;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
            => degrees * (Math.PI / 180);
    }
}