// PROPÓSITO:
//   Handler que inicia una ronda de dispatch para un OrderGroup.
//   Orquesta en dos SaveChangesAsync (mínimo necesario por FK explícita):
//     1. Carga y valida el OrderGroup.
//     2. Lee configuración desde DispatchConfig (por zona) o AppConfig (global).
//     3. Selecciona drivers elegibles: bounding-box SQL + Haversine en memoria.
//     4. Transiciona el OrderGroup a AssignedToDriver.
//     5. Crea el DispatchAttempt (ronda 1).
//     6. Primer SaveChangesAsync → persiste OrderGroup + DispatchAttempt.
//     7. Crea un DispatchAttemptDriver por cada candidato.
//     8. Registra AuditLog inmutable.
//     9. Segundo SaveChangesAsync → persiste DispatchAttemptDrivers + AuditLog.
//    10. Notifica a los drivers (fuera de transacción).
//
// PATRÓN:
//   ICommandHandler<TCommand, TResult> del mediador propio Manda2.
//   NO usa MediatR. El bus (ICommandBus) resuelve este handler por DI.
//
// DEPENDENCIAS:
//   • IApplicationDbContext  → acceso a BD (EF Core)
//   • INotificationService   → notificaciones push/SignalR (stub en MVP)
//
// INTEGRACIÓN SAP B1:
//   El AuditLog registra la ronda con todos los parámetros utilizados
//   (radio, drivers, expiración) para trazabilidad futura en ERP.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Handler para <see cref="StartDispatchCommand"/>.
    /// Inicia el proceso de asignación de repartidor para un OrderGroup,
    /// creando la ronda de dispatch y notificando a los candidatos elegibles.
    /// </summary>
    public class StartDispatchCommandHandler : ICommandHandler<StartDispatchCommand, StartDispatchResult>
    {
        // ─── DEPENDENCIAS ────────────────────────────────────────────────────
        private readonly IApplicationDbContext _db;
        private readonly INotificationService _notifications;

        // ─── CONSTANTES DE FALLBACK ──────────────────────────────────────────
        // Se usan SOLO si AppConfig no tiene los valores configurados.
        // Nunca deben quedar hardcodeados en lógica de negocio real.
        private const double DefaultInitialRadiusKm = 3.0;
        private const int DefaultMaxDriversPerRound = 5;
        private const int DefaultRoundTimeoutMinutes = 3;

        // ─── CLAVES AppConfig ────────────────────────────────────────────────
        // Deben existir en cfg.AppConfig con sus valores por defecto (Seed Data).
        private const string KeyMaxDrivers = "DISPATCH_DEFAULT_MAX_DRIVERS_PER_ROUND";
        private const string KeyInitialRadius = "DISPATCH_DEFAULT_INITIAL_RADIUS_KM";
        private const string KeyRoundTimeout = "DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES";

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// INotificationService se resuelve como NotificationServiceStub en MVP.
        /// En D3-C se reemplaza por implementación real con SignalR + Firebase.
        /// </summary>
        public StartDispatchCommandHandler(
            IApplicationDbContext db,
            INotificationService notifications)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        }

        // ─── HANDLER PRINCIPAL ───────────────────────────────────────────────

        /// <summary>
        /// Punto de entrada del handler. Orquesta el ciclo completo de dispatch:
        /// validación → configuración → selección → persistencia → notificación.
        /// </summary>
        public async Task<StartDispatchResult> HandleAsync(
            StartDispatchCommand request,
            CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Cargar OrderGroup con Stops y SubOrders ──────────────
            // Stops    → punto de referencia geográfico (primer Pickup).
            // SubOrders → validación de integridad del pedido.
            var group = await _db.OrderGroups
                .Include(og => og.Stops)
                .Include(og => og.SubOrders)
                .FirstOrDefaultAsync(og => og.Id == request.OrderGroupId, cancellationToken);

            if (group == null)
                throw new InvalidOperationException(
                    $"OrderGroup {request.OrderGroupId} no encontrado.");

            // ── PASO 2: Validar estado del OrderGroup ────────────────────────
            // Solo se puede iniciar dispatch desde AwaitingDriverAssignment.
            // Si ya está en AssignedToDriver hay una ronda activa → rechazamos
            // para evitar duplicados.
            if (group.Status != OrderGroupStatus.AwaitingDriverAssignment)
                throw new InvalidOperationException(
                    $"OrderGroup {request.OrderGroupId} no está en estado válido para dispatch. " +
                    $"Estado actual: {group.Status}. " +
                    $"Se requiere: {OrderGroupStatus.AwaitingDriverAssignment}.");

            // ── PASO 3: Determinar punto de referencia geográfico ────────────
            // Prioridad: primer Pickup stop con coordenadas válidas
            //            → DeliveryLatitude/Longitude del grupo como fallback.
            var (refLat, refLon) = GetReferencePoint(group);

            // ── PASO 4: Leer configuración de dispatch ───────────────────────
            // Jerarquía: DispatchConfig (por zona) → AppConfig (global) → constantes.
            var config = await LoadDispatchConfigAsync(request.ZoneName, cancellationToken);

            // ── PASO 5: Seleccionar drivers elegibles ────────────────────────
            // Algoritmo: bounding-box SQL (eficiencia) + Haversine en memoria (precisión).
            // Resultado: lista ordenada por distancia ASC, máximo MaxDriversPerRound.
            var candidates = await SelectCandidatesAsync(
                refLat, refLon,
                config.InitialRadiusKm,
                config.MaxDriversPerRound,
                cancellationToken);

            // ── PASO 6: Transicionar OrderGroup a AssignedToDriver ───────────
            // El grupo debe reflejar que hay una ronda activa ANTES de persistir.
            // Sin este cambio, el grupo queda en AwaitingDriverAssignment mientras
            // los drivers ya recibieron la notificación → inconsistencia de estado.
            group.Status = OrderGroupStatus.AssignedToDriver;
            group.UpdatedAt = utcNow;

            // ── PASO 7: Crear DispatchAttempt (ronda 1) ──────────────────────
            var expiresAt = utcNow.AddMinutes(config.RoundTimeoutMinutes);

            var attempt = new DispatchAttempt
            {
                OrderGroupId = group.Id,
                RoundNumber = 1,
                StartedAt = utcNow,
                ExpiresAt = expiresAt,
                Status = DispatchAttemptStatus.Sent
            };

            _db.DispatchAttempts.Add(attempt);

            // ── PASO 8: Primer SaveChangesAsync ──────────────────────────────
            // Necesario aquí porque DispatchAttemptDriver.DispatchAttemptId es
            // una FK explícita. EF Core necesita el attempt.Id generado por la BD
            // antes de poder insertar los DispatchAttemptDrivers.
            // Este save persiste: OrderGroup (estado) + DispatchAttempt.
            await _db.SaveChangesAsync(cancellationToken);

            // ── PASO 9: Crear DispatchAttemptDriver por cada candidato ────────
            // Cada registro representa la oferta enviada a un driver específico.
            // Response = Pending hasta que el driver acepte, rechace o expire.
            foreach (var (driver, distanceKm, etaMinutes) in candidates)
            {
                _db.DispatchAttemptDrivers.Add(new DispatchAttemptDriver
                {
                    DispatchAttemptId = attempt.Id,   // Id disponible tras el save anterior
                    DriverId = driver.Id,
                    NotifiedAtUtc = utcNow,
                    Response = DriverDispatchResponse.Pending,
                    DistanceKm = (decimal)distanceKm,
                    EstimatedArrivalMinutes = etaMinutes
                });
            }

            // ── PASO 10: AuditLog inmutable ──────────────────────────────────
            // Registra todos los parámetros de la ronda para trazabilidad completa.
            // Útil para soporte, forensics y futura integración SAP B1.
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroup),
                EntityId = group.Id,
                Action = "StartDispatch",
                PerformedByUserId = request.InitiatedByUserId,
                PerformedBy = request.InitiatedByUserId.HasValue
                                        ? $"UserId:{request.InitiatedByUserId}"
                                        : "System",
                Details = $"Dispatch ronda 1 creada. " +
                                      $"AttemptId={attempt.Id}. " +
                                      $"Drivers notificados={candidates.Count}. " +
                                      $"Radio={config.InitialRadiusKm}km. " +
                                      $"Timeout={config.RoundTimeoutMinutes}min. " +
                                      $"Expira={expiresAt:u}. " +
                                      $"Zona={request.ZoneName ?? "default"}.",
                CreatedAt = utcNow
            });

            // ── PASO 11: Segundo (y último) SaveChangesAsync ─────────────────
            // Persiste: DispatchAttemptDrivers + AuditLog en una sola transacción.
            // Si algo falla aquí, el rollback revierte ambas entidades juntas.
            await _db.SaveChangesAsync(cancellationToken);

            // ── PASO 12: Notificar drivers (fuera de la transacción) ─────────
            // Se ejecuta DESPUÉS del SaveChanges para garantizar que la BD
            // ya tiene el estado correcto antes de notificar.
            // Si la notificación falla, el estado en BD es correcto (no se revierte).
            // MVP: NotificationServiceStub (no-op).
            // D3-C: reemplazar por implementación real con SignalR + Firebase.
            foreach (var (driver, _, _) in candidates)
            {
                await _notifications.NotifyDriverDispatchOfferAsync(
                    driver.Id,
                    attempt.Id,
                    group.Id,
                    cancellationToken);
            }

            // ── PASO 13: Retornar resultado al controller ────────────────────
            return new StartDispatchResult
            {
                AttemptId = attempt.Id,
                NotifiedDriversCount = candidates.Count,
                ExpiresAt = expiresAt,
                Message = candidates.Count > 0
                    ? $"Dispatch iniciado. {candidates.Count} driver(s) notificados. " +
                      $"Radio: {config.InitialRadiusKm}km. Expira: {expiresAt:u}."
                    : "Dispatch iniciado pero no hay drivers disponibles en el radio configurado. " +
                      "Considere ampliar el radio o esperar disponibilidad."
            };
        }

        // ─── MÉTODOS PRIVADOS ────────────────────────────────────────────────

        /// <summary>
        /// Determina el punto de referencia geográfico para calcular distancias.
        /// Prioridad:
        ///   1. Primer Pickup stop con coordenadas válidas (≠ 0).
        ///   2. DeliveryLatitude/Longitude del OrderGroup.
        /// Lanza excepción si no hay coordenadas válidas.
        /// </summary>
        private static (double lat, double lon) GetReferencePoint(OrderGroup group)
        {
            var pickupStop = group.Stops?
                .OrderBy(s => s.Sequence)
                .FirstOrDefault(s => s.StopType == StopType.Pickup);

            if (pickupStop != null &&
                pickupStop.Latitude != 0 &&
                pickupStop.Longitude != 0)
            {
                return (Convert.ToDouble(pickupStop.Latitude),
                        Convert.ToDouble(pickupStop.Longitude));
            }

            if (group.DeliveryLatitude != 0 && group.DeliveryLongitude != 0)
            {
                return (Convert.ToDouble(group.DeliveryLatitude),
                        Convert.ToDouble(group.DeliveryLongitude));
            }

            throw new InvalidOperationException(
                $"OrderGroup {group.Id} no tiene coordenadas válidas para calcular dispatch. " +
                "Verifica que los Stops (Pickup) o DeliveryLatitude/Longitude estén poblados.");
        }

        /// <summary>
        /// Resuelve la configuración de dispatch con la siguiente jerarquía:
        ///   1. DispatchConfig por ZoneName (si se especificó zona).
        ///   2. AppConfig con claves globales DISPATCH_*.
        ///   3. Constantes de fallback (último recurso, nunca en producción).
        /// </summary>
        private async Task<DispatchConfigValues> LoadDispatchConfigAsync(
            string? zoneName,
            CancellationToken ct)
        {
            // Intentar DispatchConfig por zona
            if (!string.IsNullOrWhiteSpace(zoneName))
            {
                var zoneCfg = await _db.DispatchConfigs
                    .FirstOrDefaultAsync(d => d.ZoneName == zoneName && d.IsActive, ct);

                if (zoneCfg != null)
                {
                    return new DispatchConfigValues
                    {
                        InitialRadiusKm = Convert.ToDouble(zoneCfg.InitialRadiusKm),
                        MaxDriversPerRound = zoneCfg.MaxDriversToNotifyPerRound,
                        RoundTimeoutMinutes = zoneCfg.RoundTimeoutMinutes
                    };
                }
            }

            // Fallback: AppConfig global
            var keys = new[] { KeyMaxDrivers, KeyInitialRadius, KeyRoundTimeout };
            var appConfigs = await _db.AppConfigs
                .Where(a => keys.Contains(a.Key))
                .ToListAsync(ct);

            return new DispatchConfigValues
            {
                InitialRadiusKm = GetAppConfigDouble(appConfigs, KeyInitialRadius, DefaultInitialRadiusKm),
                MaxDriversPerRound = GetAppConfigInt(appConfigs, KeyMaxDrivers, DefaultMaxDriversPerRound),
                RoundTimeoutMinutes = GetAppConfigInt(appConfigs, KeyRoundTimeout, DefaultRoundTimeoutMinutes)
            };
        }

        /// <summary>
        /// Selecciona los drivers elegibles para esta ronda de dispatch.
        ///
        /// FASE 1 — Bounding-box SQL:
        ///   Reduce el conjunto de rows usando un rectángulo geográfico aproximado.
        ///   Rápido porque opera sobre índices de columnas.
        ///   Criterios: IsActive, IsOnline, Available, sin grupo activo, GPS válido.
        ///
        /// FASE 2 — Haversine en memoria:
        ///   Calcula la distancia exacta (círculo real) para los drivers del bounding-box.
        ///   Descarta los que están en las esquinas pero fuera del radio circular real.
        ///
        /// Resultado: lista ordenada por distancia ASC, limitada a maxDrivers.
        /// </summary>
        private async Task<List<(Manda2.Domain.Entities.Driver driver, double distanceKm, int etaMinutes)>> SelectCandidatesAsync(
            double refLat,
            double refLon,
            double radiusKm,
            int maxDrivers,
            CancellationToken ct)
        {
            // FASE 1: Bounding-box SQL
            // 1 grado de latitud  ≈ 111 km (constante).
            // 1 grado de longitud ≈ 111 km * cos(latitud) — varía con la latitud.
            double deltaLat = radiusKm / 111.0;
            double deltaLon = radiusKm / (111.0 * Math.Cos(DegreesToRadians(refLat)));

            decimal minLat = (decimal)(refLat - deltaLat);
            decimal maxLat = (decimal)(refLat + deltaLat);
            decimal minLon = (decimal)(refLon - deltaLon);
            decimal maxLon = (decimal)(refLon + deltaLon);

            // Filtro SQL: solo drivers elegibles dentro del bounding-box.
            // CurrentOrderGroupId == null → driver sin grupo activo (disponible).
            // IsOnline + Available        → driver conectado y listo para recibir ofertas.
            var candidatesInBox = await _db.Drivers
                .Where(d =>
                    d.IsActive &&
                    d.IsOnline &&
                    d.Status == DriverStatus.Available &&
                    d.CurrentOrderGroupId == null &&
                    d.LastLatitude != null &&
                    d.LastLongitude != null &&
                    d.LastLatitude >= minLat &&
                    d.LastLatitude <= maxLat &&
                    d.LastLongitude >= minLon &&
                    d.LastLongitude <= maxLon)
                .ToListAsync(ct);

            // FASE 2: Haversine en memoria + filtro exacto
            var scored = new List<(Manda2.Domain.Entities.Driver driver, double distanceKm, int etaMinutes)>();

            foreach (var driver in candidatesInBox)
            {
                double driverLat = Convert.ToDouble(driver.LastLatitude!.Value);
                double driverLon = Convert.ToDouble(driver.LastLongitude!.Value);

                double dist = HaversineDistanceKm(refLat, refLon, driverLat, driverLon);

                // Descartar drivers en las esquinas del bounding-box
                // que están fuera del radio circular real.
                if (dist > radiusKm) continue;

                // ETA estimado: velocidad promedio urbana 25 km/h.
                // Fórmula: (distancia / velocidad) * 60 minutos.
                // Reemplazar por Google Maps Distance Matrix API en D3-C.
                int eta = (int)Math.Ceiling((dist / 25.0) * 60.0);

                scored.Add((driver, dist, eta));
            }

            // Ordenar por distancia ASC → los más cercanos primero.
            // Tomar solo los N primeros según configuración.
            return scored
                .OrderBy(x => x.distanceKm)
                .Take(maxDrivers)
                .ToList();
        }

        // ─── HELPERS MATEMÁTICOS ─────────────────────────────────────────────

        /// <summary>
        /// Fórmula de Haversine para distancia entre dos coordenadas GPS.
        /// Retorna kilómetros. Más precisa que distancia euclidiana para
        /// puntos geográficos separados por varios kilómetros.
        /// </summary>
        private static double HaversineDistanceKm(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371.0; // Radio medio de la Tierra en km

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(DegreesToRadians(lat1))
                     * Math.Cos(DegreesToRadians(lat2))
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double degrees)
            => degrees * Math.PI / 180.0;

        // ─── HELPERS AppConfig ───────────────────────────────────────────────

        /// <summary>
        /// Lee un valor double desde la lista de AppConfig cargada.
        /// Retorna el fallback si la clave no existe o el valor no es parseable.
        /// </summary>
        private static double GetAppConfigDouble(
            List<AppConfig> configs, string key, double fallback)
        {
            var val = configs.FirstOrDefault(c => c.Key == key)?.Value;
            return double.TryParse(
                val,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result)
                ? result
                : fallback;
        }

        /// <summary>
        /// Lee un valor int desde la lista de AppConfig cargada.
        /// Retorna el fallback si la clave no existe o el valor no es parseable.
        /// </summary>
        private static int GetAppConfigInt(
            List<AppConfig> configs, string key, int fallback)
        {
            var val = configs.FirstOrDefault(c => c.Key == key)?.Value;
            return int.TryParse(val, out var result) ? result : fallback;
        }

        // ─── DTO INTERNO ─────────────────────────────────────────────────────

        /// <summary>
        /// DTO interno que encapsula los valores de configuración de dispatch
        /// resueltos desde DispatchConfig o AppConfig.
        /// Evita pasar múltiples parámetros entre métodos privados.
        /// </summary>
        private sealed class DispatchConfigValues
        {
            /// <summary>Radio inicial de búsqueda en kilómetros.</summary>
            public double InitialRadiusKm { get; set; }

            /// <summary>Máximo de drivers a notificar en esta ronda.</summary>
            public int MaxDriversPerRound { get; set; }

            /// <summary>Minutos antes de que expire esta ronda sin aceptación.</summary>
            public int RoundTimeoutMinutes { get; set; }
        }
    }
}