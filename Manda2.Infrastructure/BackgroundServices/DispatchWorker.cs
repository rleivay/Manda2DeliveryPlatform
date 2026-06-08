// PROPÓSITO: BackgroundService que mantiene vivo el ciclo de despacho.
//
// TAREAS:
//   1. Expirar DispatchAttempts cuyo ExpiresAt ya pasó (Status = Sent).
//   2. Expirar DispatchAttemptDrivers pendientes dentro de esos intentos.
//   3. Detectar OrderGroups en AwaitingDriverAssignment sin intento activo
//      → listos para nueva ronda (Item 11: algoritmo de ruteo).
// CAMBIO v2: DetectGroupsNeedingNewRound ahora invoca ExpandDispatchCommand
//            a través de ICommandBus en lugar de solo loguear (HOOK → REAL).
//
// FLUJO COMPLETO DEL WORKER:
//   Ciclo cada N segundos:
//   1. ExpireAttempts        → Marca Sent→Expired los intentos con ExpiresAt < now.
//                              Expira también los DispatchAttemptDrivers Pending.
//   2. DetectGroupsNeedingNewRound → Detecta grupos AwaitingDriverAssignment
//                              sin intento activo → invoca ExpandDispatchCommand.
//
// PATRÓN:
//   BackgroundService (Singleton) → crea IServiceScope por ciclo
//   para resolver IApplicationDbContext y ICommandBus (ambos Scoped).

// PATRÓN:
//   - BackgroundService es Singleton → crea IServiceScope por ciclo
//     para resolver IApplicationDbContext (Scoped).
//   - Intervalo configurable vía appsettings: "Dispatch:WorkerIntervalSeconds"
//
// ENUMS REALES USADOS:
//   - OrderGroupStatus.AwaitingDriverAssignment
//   - DispatchEnums.DispatchAttemptStatus.Sent / Expired
//   - DispatchEnums.DriverDispatchResponse.Pending / Expired
//   - DriverStatus → se consulta antes de modificar (Item 11)
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Worker de despacho. Corre en segundo plano cada N segundos.
    /// Mantiene la consistencia del ciclo de dispatch sin intervención manual.
    /// </summary>
    public class DispatchWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DispatchWorker> _logger;
        private readonly IConfiguration _config;

        public DispatchWorker(
            IServiceProvider serviceProvider,
            ILogger<DispatchWorker> logger,
            IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[DispatchWorker] Iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Scope manual: BackgroundService es Singleton,
                    // IApplicationDbContext e ICommandBus son Scoped.
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                    var bus = scope.ServiceProvider.GetRequiredService<ICommandBus>();

                    int intervalSeconds = int.Parse(
                        _config["Dispatch:WorkerIntervalSeconds"] ?? "10");

                    await ExpireAttempts(db, stoppingToken);
                    await DetectGroupsNeedingNewRound(db, bus, stoppingToken);

                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Apagado limpio
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DispatchWorker] Error en ciclo de despacho.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("[DispatchWorker] Detenido.");
        }

        // ════════════════════════════════════════════════════════════════════
        // TAREA 1: Expirar intentos y sus drivers pendientes
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca DispatchAttempts con Status = Sent cuyo ExpiresAt ya pasó.
        /// Los marca como Expired y expira también los DispatchAttemptDrivers
        /// que aún estén en Pending dentro de ese intento.
        /// </summary>
        private async Task ExpireAttempts(
            IApplicationDbContext db, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var expiredAttempts = await db.DispatchAttempts
                .Include(a => a.NotifiedDrivers)
                .Where(a => a.Status == DispatchAttemptStatus.Sent
                         && a.ExpiresAt < now
                         && !a.IsDeleted)
                .ToListAsync(ct);

            if (!expiredAttempts.Any()) return;

            foreach (var attempt in expiredAttempts)
            {
                _logger.LogInformation(
                    "[DispatchWorker] Expirando intento {AttemptId} " +
                    "(Ronda {Round}) del Grupo {GroupId}.",
                    attempt.Id, attempt.RoundNumber, attempt.OrderGroupId);

                attempt.Status = DispatchAttemptStatus.Expired;
                attempt.UpdatedAt = now;

                foreach (var dad in attempt.NotifiedDrivers
                    .Where(d => d.Response == DriverDispatchResponse.Pending))
                {
                    dad.Response = DriverDispatchResponse.Expired;
                    dad.RespondedAtUtc = now;
                    dad.UpdatedAt = now;
                }
            }

            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[DispatchWorker] {Count} intento(s) expirado(s).",
                expiredAttempts.Count);
        }

        // ════════════════════════════════════════════════════════════════════
        // TAREA 2: Detectar grupos huérfanos → invocar ExpandDispatchCommand
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca OrderGroups en AwaitingDriverAssignment sin intento activo (Sent).
        /// Para cada uno, obtiene el último DispatchAttempt expirado e invoca
        /// ExpandDispatchCommand para iniciar la siguiente ronda.
        ///
        /// Si el grupo nunca tuvo intentos (LastAttemptId = null), se loguea
        /// como anomalía — StartDispatchCommand debería haberlo iniciado.
        /// </summary>
        private async Task DetectGroupsNeedingNewRound(
            IApplicationDbContext db,
            ICommandBus bus,
            CancellationToken ct)
        {
            // Proyectamos Id + último intento expirado del grupo.
            // Solo grupos sin ningún intento activo (Sent).
            var groupsWaiting = await db.OrderGroups
                .Where(g => g.Status == OrderGroupStatus.AwaitingDriverAssignment
                         && !g.IsDeleted)
                .Where(g => !db.DispatchAttempts
                    .Any(a => a.OrderGroupId == g.Id
                           && a.Status == DispatchAttemptStatus.Sent
                           && !a.IsDeleted))
                .Select(g => new
                {
                    g.Id,
                    // Último intento expirado: el que ExpandDispatch marcará como previo.
                    LastAttemptId = db.DispatchAttempts
                        .Where(a => a.OrderGroupId == g.Id && !a.IsDeleted)
                        .OrderByDescending(a => a.RoundNumber)
                        .Select(a => (int?)a.Id)
                        .FirstOrDefault(),
                    LastRound = db.DispatchAttempts
                        .Where(a => a.OrderGroupId == g.Id && !a.IsDeleted)
                        .Select(a => (int?)a.RoundNumber)
                        .Max() ?? 0
                })
                .ToListAsync(ct);

            if (!groupsWaiting.Any()) return;

            foreach (var group in groupsWaiting)
            {
                // Anomalía: grupo en AwaitingDriverAssignment sin ningún intento previo.
                // StartDispatchCommand debería haberlo iniciado. Loguear y saltar.
                if (group.LastAttemptId == null)
                {
                    _logger.LogWarning(
                        "[DispatchWorker] Grupo {GroupId} está en AwaitingDriverAssignment " +
                        "pero nunca tuvo un DispatchAttempt. Revisar flujo de StartDispatch.",
                        group.Id);
                    continue;
                }

                _logger.LogInformation(
                    "[DispatchWorker] Grupo {GroupId} necesita nueva ronda " +
                    "(última ronda: {LastRound}, último intento: {AttemptId}). " +
                    "Invocando ExpandDispatchCommand.",
                    group.Id, group.LastRound, group.LastAttemptId);

                try
                {
                    // ── Invocar ExpandDispatchCommand vía ICommandBus ──────────
                    // ExpandDispatchCommandHandler se encarga de:
                    //   - Verificar MaxRounds → si se alcanzó, pasa a AwaitingManualAssignment.
                    //   - Calcular nuevo radio.
                    //   - Seleccionar candidatos excluyendo rechazos previos.
                    //   - Crear nuevo DispatchAttempt + DispatchAttemptDrivers.
                    //   - Notificar drivers.
                    var result = await bus.SendAsync<ExpandDispatchCommand, ExpandDispatchResult>(
                        new ExpandDispatchCommand
                        {
                            OrderGroupId = group.Id,
                            PreviousAttemptId = group.LastAttemptId.Value
                        }, ct);

                    if (result.NewAttemptCreated)
                    {
                        _logger.LogInformation(
                            "[DispatchWorker] Grupo {GroupId} → Ronda {Round} iniciada. " +
                            "AttemptId: {AttemptId}. Drivers: {Count}. Radio: {Radius}km.",
                            group.Id,
                            result.NewRoundNumber,
                            result.NewAttemptId,
                            result.DriversNotified,
                            result.RadiusUsedKm);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "[DispatchWorker] Grupo {GroupId} → No se creó nueva ronda. " +
                            "Motivo: {Message}",
                            group.Id, result.Message);
                    }
                }
                catch (Exception ex)
                {
                    // Error aislado por grupo — no detiene el ciclo del worker.
                    _logger.LogError(ex,
                        "[DispatchWorker] Error al expandir dispatch del Grupo {GroupId}.",
                        group.Id);
                }
            }
        }
    }
}
