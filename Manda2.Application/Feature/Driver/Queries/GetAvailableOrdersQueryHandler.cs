// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Queries/GetAvailableOrders/
//          GetAvailableOrdersHandler.cs
//
// PROPÓSITO: Lógica de negocio para encontrar grupos de órdenes disponibles
//            cercanos al driver, respetando:
//            - Radio de búsqueda configurable (AppConfig)
//            - Solo grupos en estado AwaitingDriverAssignment
//            - Solo grupos donde el driver fue notificado (DispatchAttemptDriver)
//            - Filtro por distancia Haversine en memoria
//
// PATRÓN: IQueryHandler<TQuery, TResult> del mediador propio.
// ═══════════════════════════════════════════════════════════════════════════
using Manda2.Application.Common;
using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Driver.Queries
{
    public class GetAvailableOrdersHandler
        : IQueryHandler<GetAvailableOrdersQuery, List<OrderGroupSummaryDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetAvailableOrdersHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<OrderGroupSummaryDto>> HandleAsync(
            GetAvailableOrdersQuery query, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Leer radio de búsqueda desde AppConfig
            // Key: DISPATCH_DEFAULT_INITIAL_RADIUS_KM
            // Fallback: 3 km si la config no existe (valor conservador urbano)
            // ─────────────────────────────────────────────────────────────
            var radiusConfig = await _db.AppConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Key == "DISPATCH_DEFAULT_INITIAL_RADIUS_KM", ct);

            var radiusKm = decimal.TryParse(radiusConfig?.Value, out var r) ? r : 3m;

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Leer timeout de ronda para calcular ExpiresInSeconds
            // Key: DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES
            // Fallback: 2 minutos
            // ─────────────────────────────────────────────────────────────
            var timeoutConfig = await _db.AppConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Key == "DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES", ct);

            var timeoutMinutes = int.TryParse(timeoutConfig?.Value, out var t) ? t : 2;

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Traer grupos en estado AwaitingDriverAssignment
            //         con sus paradas, SubOrders y el intento de dispatch activo.
            //
            // NOTA: Usamos AsNoTracking() porque es solo lectura.
            //       Incluimos solo los DispatchAttempts con estado Sent
            //       para no cargar historial innecesario.
            // ─────────────────────────────────────────────────────────────
            var groups = await _db.OrderGroups
                .AsNoTracking()
                .Include(og => og.Stops.OrderBy(s => s.Sequence))
                .Include(og => og.SubOrders)
                    .ThenInclude(s => s.Merchant)
                .Include(og => og.DispatchAttempts
                    .Where(da => da.Status == DispatchAttemptStatus.Sent))
                    .ThenInclude(da => da.NotifiedDrivers
                        .Where(nd => nd.DriverId == query.DriverId))
                .Where(og => og.Status == OrderGroupStatus.AwaitingDriverAssignment)
                .ToListAsync(ct);

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Filtrar en memoria por:
            //         a) El driver fue notificado en el intento activo
            //         b) La distancia al primer Pickup está dentro del radio
            // ─────────────────────────────────────────────────────────────
            var result = new List<OrderGroupSummaryDto>();

            foreach (var group in groups)
            {
                // a) Verificar que el driver fue notificado para este grupo
                var notification = group.DispatchAttempts
                    .SelectMany(da => da.NotifiedDrivers)
                    .FirstOrDefault(nd => nd.DriverId == query.DriverId);

                // Si no fue notificado, este grupo no le corresponde
                if (notification == null) continue;

                // b) Obtener la primera parada (Pickup del primer comercio)
                //    para calcular distancia desde el driver
                var firstStop = group.Stops.FirstOrDefault();
                if (firstStop == null) continue;

                // c) Calcular distancia Haversine driver → primer Pickup
                var distanceKm = CalculateHaversineKm(
                    (double)query.CurrentLatitude,
                    (double)query.CurrentLongitude,
                    (double)firstStop.Latitude,
                    (double)firstStop.Longitude);

                // d) Descartar si está fuera del radio configurado
                if ((decimal)distanceKm > radiusKm) continue;

                // ─────────────────────────────────────────────────────────
                // PASO 5: Calcular segundos restantes para que expire la oferta
                // Fórmula: NotifiedAtUtc + timeoutMinutes - UtcNow
                // Si ya expiró, ExpiresInSeconds = 0 (la app oculta el pedido)
                // ─────────────────────────────────────────────────────────
                var expiresAt = notification.NotifiedAtUtc.AddMinutes(timeoutMinutes);
                var expiresInSeconds = Math.Max(0, (int)(expiresAt - DateTime.UtcNow).TotalSeconds);

                // ─────────────────────────────────────────────────────────
                // PASO 6: Construir el DTO de resumen para la app del Driver
                // ─────────────────────────────────────────────────────────
                result.Add(new OrderGroupSummaryDto
                {
                    OrderGroupId = group.Id,
                    MerchantCount = group.SubOrders.Count,
                    EstimatedDistanceKm = Math.Round((decimal)distanceKm, 2),
                    EstimatedTotalMinutes = EstimateRouteMinutes(group.Stops.Count, distanceKm),
                    TotalAmount = group.TotalAmount,
                    PaymentMethodName = group.Payments
        .FirstOrDefault()?.PaymentMethod?.Name ?? "Efectivo",
                    DeliveryAddressText = group.DeliveryAddressText ?? string.Empty,
                    ExpiresInSeconds = expiresInSeconds,

                    Stops = group.Stops.Select(s => new StopSummaryDto
                    {
                        Sequence = s.Sequence,
                        StopType = s.StopType.ToString(),
                        Label = s.StopType == StopType.Pickup
                            ? (s.Merchant?.Name ?? "Comercio")
                            : "Entrega al cliente",
                        AddressText = s.AddressText ?? string.Empty,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude
                    }).ToList()
                });
            }

            // Ordenar por distancia ascendente: el más cercano aparece primero
            return result.OrderBy(r => r.EstimatedDistanceKm).ToList();
        }

        // ─────────────────────────────────────────────────────────────────
        // HELPER: CalculateHaversineKm
        //
        // Fórmula matemática para distancia real entre dos coordenadas GPS.
        // Más precisa que distancia euclidiana para puntos geográficos.
        //
        // Parámetros: lat/lon en grados decimales
        // Retorna: distancia en kilómetros
        // ─────────────────────────────────────────────────────────────────
        private static double CalculateHaversineKm(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371; // Radio medio de la Tierra en km
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;

        // ─────────────────────────────────────────────────────────────────
        // HELPER: EstimateRouteMinutes
        //
        // Estimación simple del tiempo total de la ruta.
        // Fórmula: (distancia / velocidad urbana promedio) + (paradas × espera)
        //
        // Velocidad urbana promedio: 25 km/h
        // Tiempo de espera por parada (pickup en comercio): 5 minutos
        // ─────────────────────────────────────────────────────────────────
        private static int EstimateRouteMinutes(int stopCount, double distanceKm)
        {
            const double avgSpeedKmH = 25.0;
            const int minutesPerStop = 5;

            var travelMinutes = (distanceKm / avgSpeedKmH) * 60;
            return (int)Math.Ceiling(travelMinutes + (stopCount * minutesPerStop));
        }
    }
}
