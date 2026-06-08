// PROPÓSITO:
//   Handler que escala la búsqueda de driver a la siguiente ronda cuando:
//   a) La ronda anterior expiró por timeout (DispatchWorker lo detecta).
//   b) Todos los drivers notificados rechazaron la oferta.
//
// LÓGICA DE NEGOCIO:
//   1. Carga el OrderGroup con Stops, SubOrders y DispatchAttempts.
//   2. Valida que el intento previo existe y pertenece al grupo.
//   3. Verifica que no se superó MaxRounds (de DispatchConfig o AppConfig fallback).
//   4. Marca el intento anterior como Expired.
//   5. Calcula el nuevo radio: InitialRadiusKm + ((RoundNumber-1) × RadiusExpansionFactor)
//   6. Selecciona candidatos EXCLUYENDO drivers que ya rechazaron/expiraron.
//   7. Crea el nuevo DispatchAttempt con sus DispatchAttemptDrivers.
//   8. Notifica a los drivers seleccionados.
//   9. Registra AuditLog.
//
// FÓRMULA DE RADIO:
//   Radio(N) = InitialRadiusKm + ((N-1) × RadiusExpansionFactor)
//   Seed Nicaragua: R1=3km, R2=5km, R3=7km
//
// CONSUMIDOR:
//   - DispatchWorker (timeout de ronda)
//   - RejectDispatchCommandHandler (todos rechazaron)
//
// PATRÓN: ICommandHandler<TCommand, TResult> — ICommandBus propio (NO MediatR)
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Feature.Dispatch.Dtos;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    // ────────────────────────────────────────────────────────────────────────
    // HANDLER
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Handler que orquesta la expansión del dispatch a la siguiente ronda.
    /// </summary>
    public class ExpandDispatchCommandHandler
        : ICommandHandler<ExpandDispatchCommand, ExpandDispatchResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly INotificationService _notifications;
        private readonly ILogger<ExpandDispatchCommandHandler> _logger;

        public ExpandDispatchCommandHandler(
            IApplicationDbContext db,
            INotificationService notifications,
            ILogger<ExpandDispatchCommandHandler> logger)
        {
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<ExpandDispatchResult> HandleAsync(
            ExpandDispatchCommand request,
            CancellationToken ct)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Cargar OrderGroup con Stops, SubOrders y DispatchAttempts ──
            // Stops      → punto de referencia GPS (primer Pickup).
            // SubOrders  → MerchantIds para notificación de cambio de estado.
            // DispatchAttempts → validar intento previo sin query adicional.
            var group = await _db.OrderGroups
                .Include(og => og.Stops)
                .Include(og => og.SubOrders)           // ← AGREGADO
                .Include(og => og.DispatchAttempts)
                .FirstOrDefaultAsync(og => og.Id == request.OrderGroupId, ct);

            if (group == null)
            {
                _logger.LogWarning(
                    "[ExpandDispatch] OrderGroup {OrderGroupId} no encontrado.",
                    request.OrderGroupId);
                return Fail("OrderGroup no encontrado.");
            }

            // ── PASO 2: Validar intento previo ────────────────────────────────
            var prevAttempt = group.DispatchAttempts
                .FirstOrDefault(da => da.Id == request.PreviousAttemptId);

            if (prevAttempt == null)
            {
                _logger.LogWarning(
                    "[ExpandDispatch] DispatchAttempt {AttemptId} no pertenece al grupo {GroupId}.",
                    request.PreviousAttemptId, request.OrderGroupId);
                return Fail("Intento de dispatch previo no encontrado en el grupo.");
            }

            // ── PASO 3: Cargar configuración de dispatch ──────────────────────
            var config = await LoadDispatchConfigAsync(ct);

            // ── PASO 4: Verificar límite de rondas ────────────────────────────
            if (prevAttempt.RoundNumber >= config.MaxRounds)
            {
                _logger.LogWarning(
                    "[ExpandDispatch] OrderGroup {GroupId} alcanzó el máximo de rondas ({MaxRounds}). " +
                    "Pasando a AwaitingManualAssignment.",
                    group.Id, config.MaxRounds);

                prevAttempt.Status = DispatchAttemptStatus.Expired;
                group.Status = OrderGroupStatus.AwaitingManualAssignment;  // ← valor 13

                // Notificar al cliente y a los merchants del grupo
                var merchantIds = group.SubOrders?
                    .Select(s => s.MerchantId)
                    ?? Enumerable.Empty<int>();

                await _notifications.NotifyOrderGroupStatusChangedAsync(
                    group.Id,
                    OrderGroupStatus.AwaitingManualAssignment.ToString(),
                    group.CustomerId,
                    merchantIds,
                    ct);

                _db.AuditLogs.Add(new AuditLog
                {
                    EntityName = nameof(OrderGroup),
                    EntityId = group.Id,
                    Action = "DispatchMaxRoundsReached",
                    Details = $"Ronda {prevAttempt.RoundNumber}/{config.MaxRounds} expirada. " +
                              $"Grupo {group.Id} → AwaitingManualAssignment.",
                    CreatedAt = utcNow
                });

                await _db.SaveChangesAsync(ct);

                return new ExpandDispatchResult
                {
                    NewAttemptCreated = false,
                    Message = $"Máximo de rondas ({config.MaxRounds}) alcanzado. " +
                              "Pedido pasado a gestión manual (BackOffice)."
                };
            }

            // ── PASO 5: Calcular radio de la nueva ronda ──────────────────────
            // Fórmula: Radio(N) = InitialRadiusKm + ((N-1) × RadiusExpansionFactor)
            int newRoundNumber = prevAttempt.RoundNumber + 1;
            decimal newRadiusKm = config.InitialRadiusKm
                + ((newRoundNumber - 1) * config.RadiusExpansionFactor);

            _logger.LogInformation(
                "[ExpandDispatch] OrderGroup {GroupId} → Ronda {Round} | Radio {Radius}km",
                group.Id, newRoundNumber, newRadiusKm);

            // ── PASO 6: Obtener punto de referencia GPS ───────────────────────
            var refPoint = GetReferencePoint(group);
            if (refPoint == null)
            {
                _logger.LogError(
                    "[ExpandDispatch] OrderGroup {GroupId} no tiene paradas Pickup con GPS válido.",
                    group.Id);
                return Fail("No se pudo determinar el punto de referencia GPS del pedido.");
            }

            // ── PASO 7: Obtener IDs de drivers a EXCLUIR ─────────────────────
            // Excluimos drivers que ya rechazaron o expiraron en CUALQUIER ronda
            // anterior del mismo OrderGroup.
            var excludedDriverIds = await _db.DispatchAttemptDrivers
                .Where(dad =>
                    dad.DispatchAttempt.OrderGroupId == request.OrderGroupId
                    && (dad.Response == DriverDispatchResponse.Rejected
                        || dad.Response == DriverDispatchResponse.Expired))
                .Select(dad => dad.DriverId)
                .Distinct()
                .ToListAsync(ct);

            _logger.LogInformation(
                "[ExpandDispatch] Excluyendo {Count} drivers que ya rechazaron/expiraron.",
                excludedDriverIds.Count);

            // ── PASO 8: Seleccionar candidatos en el nuevo radio ──────────────
            var candidates = await SelectCandidatesAsync(
                refPoint.Value.lat,
                refPoint.Value.lon,
                newRadiusKm,
                config.MaxDriversToNotifyPerRound,
                excludedDriverIds,
                ct);

            // ── PASO 9: Marcar intento anterior como Expired ──────────────────
            prevAttempt.Status = DispatchAttemptStatus.Expired;

            // ── PASO 10: Crear nuevo DispatchAttempt ──────────────────────────
            var newAttempt = new DispatchAttempt
            {
                OrderGroupId = group.Id,
                RoundNumber = newRoundNumber,
                StartedAt = utcNow,
                ExpiresAt = utcNow.AddMinutes(config.RoundTimeoutMinutes),
                Status = DispatchAttemptStatus.Sent
            };

            _db.DispatchAttempts.Add(newAttempt);

            // SaveChanges para obtener el ID del nuevo intento antes de crear los drivers
            await _db.SaveChangesAsync(ct);

            // ── PASO 11: Crear DispatchAttemptDriver por cada candidato ───────
            foreach (var driver in candidates)
            {
                _db.DispatchAttemptDrivers.Add(new DispatchAttemptDriver
                {
                    DispatchAttemptId = newAttempt.Id,
                    DriverId = driver.DriverId,
                    NotifiedAtUtc = utcNow,
                    DistanceKm = driver.DistanceKm,
                    EstimatedArrivalMinutes = driver.EstimatedArrivalMinutes,
                    Response = DriverDispatchResponse.Pending
                });
            }

            // ── PASO 12: AuditLog ─────────────────────────────────────────────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroup),
                EntityId = group.Id,
                Action = "DispatchExpanded",
                Details = $"Ronda {newRoundNumber} iniciada. Radio: {newRadiusKm}km. " +
                          $"Drivers notificados: {candidates.Count}. " +
                          $"Drivers excluidos: {excludedDriverIds.Count}.",
                CreatedAt = utcNow
            });

            await _db.SaveChangesAsync(ct);

            // ── PASO 13: Notificar a cada driver seleccionado ─────────────────
            foreach (var driver in candidates)
            {
                await _notifications.NotifyDriverDispatchOfferAsync(
                    driver.DriverId,
                    newAttempt.Id,
                    group.Id,
                    ct);
            }

            _logger.LogInformation(
                "[ExpandDispatch] Ronda {Round} creada para OrderGroup {GroupId}. " +
                "Attempt ID: {AttemptId}. Drivers: {Count}.",
                newRoundNumber, group.Id, newAttempt.Id, candidates.Count);

            return new ExpandDispatchResult
            {
                NewAttemptCreated = true,
                NewAttemptId = newAttempt.Id,
                NewRoundNumber = newRoundNumber,
                RadiusUsedKm = newRadiusKm,
                DriversNotified = candidates.Count,
                Message = $"Ronda {newRoundNumber} iniciada. " +
                          $"Radio: {newRadiusKm}km. " +
                          $"Drivers notificados: {candidates.Count}."
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODOS PRIVADOS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Carga la configuración de dispatch.
        /// Jerarquía: DispatchConfig activo → AppConfig global → valores defensivos.
        /// </summary>
        private async Task<DispatchConfigDto> LoadDispatchConfigAsync(CancellationToken ct)
        {
            var zoneConfig = await _db.DispatchConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(dc => dc.IsActive, ct);

            if (zoneConfig != null)
            {
                return new DispatchConfigDto
                {
                    InitialRadiusKm = zoneConfig.InitialRadiusKm,
                    RadiusExpansionFactor = zoneConfig.RadiusExpansionFactor,
                    MaxDriversToNotifyPerRound = zoneConfig.MaxDriversToNotifyPerRound,
                    RoundTimeoutMinutes = zoneConfig.RoundTimeoutMinutes,
                    MaxRounds = zoneConfig.MaxRounds
                };
            }

            var configs = await _db.AppConfigs
                .AsNoTracking()
                .Where(c => c.Key.StartsWith("DISPATCH_DEFAULT_"))
                .ToListAsync(ct);

            decimal GetDecimal(string key, decimal fallback) =>
                decimal.TryParse(configs.FirstOrDefault(c => c.Key == key)?.Value,
                    out var v) ? v : fallback;

            int GetInt(string key, int fallback) =>
                int.TryParse(configs.FirstOrDefault(c => c.Key == key)?.Value,
                    out var v) ? v : fallback;

            return new DispatchConfigDto
            {
                InitialRadiusKm = GetDecimal("DISPATCH_DEFAULT_INITIAL_RADIUS_KM", 3m),
                RadiusExpansionFactor = GetDecimal("DISPATCH_DEFAULT_RADIUS_EXPANSION_FACTOR", 2m),
                MaxDriversToNotifyPerRound = GetInt("DISPATCH_DEFAULT_MAX_DRIVERS_PER_ROUND", 5),
                RoundTimeoutMinutes = GetInt("DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES", 2),
                MaxRounds = GetInt("DISPATCH_DEFAULT_MAX_ROUNDS", 3)
            };
        }

        /// <summary>
        /// Obtiene el punto de referencia GPS del pedido.
        /// Usa el primer Pickup ordenado por Sequence como punto de origen.
        /// </summary>
        private static (decimal lat, decimal lon)? GetReferencePoint(OrderGroup group)
        {
            var firstPickup = group.Stops?
                .Where(s => s.StopType == StopType.Pickup)
                .OrderBy(s => s.Sequence)
                .FirstOrDefault();

            if (firstPickup == null
                || firstPickup.Latitude == 0
                || firstPickup.Longitude == 0)
                return null;

            return (firstPickup.Latitude, firstPickup.Longitude);
        }

        /// <summary>
        /// Selecciona drivers candidatos dentro del radio especificado,
        /// excluyendo los que ya rechazaron o expiraron en rondas anteriores.
        /// Ordena por distancia ascendente (más cercano primero).
        /// </summary>
        private async Task<List<DriverCandidateDto>> SelectCandidatesAsync(
            decimal refLat,
            decimal refLon,
            decimal radiusKm,
            int maxDrivers,
            List<int> excludedDriverIds,
            CancellationToken ct)
        {
            decimal degreeOffset = radiusKm / 111.32m;

            var candidates = await _db.Drivers
                .AsNoTracking()
                .Where(d =>
                    d.IsActive
                    && d.IsOnline
                    && d.Status == DriverStatus.Available
                    && d.CurrentOrderGroupId == null
                    && !excludedDriverIds.Contains(d.Id)
                    && d.LastLatitude >= refLat - degreeOffset
                    && d.LastLatitude <= refLat + degreeOffset
                    && d.LastLongitude >= refLon - degreeOffset
                    && d.LastLongitude <= refLon + degreeOffset)
                .Select(d => new { d.Id, d.LastLatitude, d.LastLongitude })
                .ToListAsync(ct);

            return candidates
                .Select(d =>
                {
                    var dist = HaversineKm(
                        (double)refLat, (double)refLon,
                        (double)d.LastLatitude, (double)d.LastLongitude);

                    return new DriverCandidateDto
                    {
                        DriverId = d.Id,
                        DistanceKm = (decimal)dist,
                        EstimatedArrivalMinutes = (int)Math.Ceiling(dist / 0.5)
                    };
                })
                .Where(d => d.DistanceKm <= radiusKm)
                .OrderBy(d => d.DistanceKm)
                .Take(maxDrivers)
                .ToList();
        }

        /// <summary>
        /// Fórmula de Haversine para calcular distancia entre dos puntos GPS.
        /// Retorna distancia en kilómetros.
        /// </summary>
        private static double HaversineKm(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;

        private static ExpandDispatchResult Fail(string message) =>
            new() { NewAttemptCreated = false, Message = message };

        // ── DTOs internos ────────────────────────────────────────────────────

        private class DispatchConfigDto
        {
            public decimal InitialRadiusKm { get; set; }
            public decimal RadiusExpansionFactor { get; set; }
            public int MaxDriversToNotifyPerRound { get; set; }
            public int RoundTimeoutMinutes { get; set; }
            public int MaxRounds { get; set; }
        }

        private class DriverCandidateDto
        {
            public int DriverId { get; set; }
            public decimal DistanceKm { get; set; }
            public int EstimatedArrivalMinutes { get; set; }
        }
    }
}
