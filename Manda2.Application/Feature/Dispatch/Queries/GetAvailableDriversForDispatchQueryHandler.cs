// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: GetAvailableDriversForDispatchQueryHandler.cs
//
// PROPÓSITO: Ejecuta la consulta de drivers elegibles para dispatch.
//
// ALGORITMO DE DISTANCIA:
//   Usamos la fórmula de Haversine para calcular distancia real entre
//   dos coordenadas geográficas sobre la superficie terrestre.
//   Es más precisa que la distancia euclidiana para coordenadas GPS.
//
// NOTA ARQUITECTÓNICA:
//   El filtro de radio se aplica en memoria (post-query) porque SQL Server
//   no tiene Haversine nativo sin extensiones. En volumen alto (>500 drivers
//   activos simultáneos) se recomienda migrar a PostGIS o Azure SQL Spatial.
//   Para el MVP de Nicaragua, este approach es más que suficiente.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Feature.Dispatch.Dtos;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Manda2.Application.Feature.Dispatch.Queries
{
    public class GetAvailableDriversForDispatchQueryHandler
        : IQueryHandler<GetAvailableDriversForDispatchQuery, List<AvailableDriverDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAvailableDriversForDispatchQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AvailableDriverDto>> HandleAsync(
            GetAvailableDriversForDispatchQuery query, CancellationToken ct)
        {
            // ── Umbral de GPS válido ──────────────────────────────────────
            var staleThreshold = DateTime.UtcNow.AddMinutes(-query.StaleLocationMinutes);

            // ── Candidatos base desde DB ──────────────────────────────────
            // Aplicamos todos los filtros booleanos en SQL para reducir el
            // conjunto antes de traer a memoria para el cálculo de distancia.
            var candidates = await _context.Drivers
                .Where(d =>
                    d.IsActive == true &&
                    d.IsOnline == true &&
                    d.IsCashBlocked == false &&
                    d.CurrentOrderGroupId == null &&
                    d.LastLatitude != null &&
                    d.LastLongitude != null &&
                    d.LastLocationUpdateAt != null &&
                    d.LastLocationUpdateAt >= staleThreshold)
                .Select(d => new
                {
                    d.Id,
                    FullName = $"{d.FirstName} {d.LastName}",
                    Lat = d.LastLatitude!.Value,
                    Lon = d.LastLongitude!.Value,
                    d.LastLocationUpdateAt
                })
                .ToListAsync(ct);

            // ── Filtro por radio con Haversine ────────────────────────────
            var result = candidates
                .Select(d => new AvailableDriverDto
                {
                    DriverId = d.Id,
                    FullName = d.FullName,
                    LastLatitude = d.Lat,
                    LastLongitude = d.Lon,
                    LastLocationUpdateAt = d.LastLocationUpdateAt!.Value,
                    DistanceKm = Haversine(
                        (double)query.PickupLatitude,
                        (double)query.PickupLongitude,
                        (double)d.Lat,
                        (double)d.Lon)
                })
                .Where(d => d.DistanceKm <= (double)query.RadiusKm)
                .OrderBy(d => d.DistanceKm)   // El más cercano primero
                .Take(query.MaxDrivers)
                .ToList();

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        // FÓRMULA DE HAVERSINE
        // Calcula la distancia en kilómetros entre dos puntos GPS.
        // Radio de la Tierra: 6371 km
        // ─────────────────────────────────────────────────────────────────
        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * (Math.PI / 180);
    }
}