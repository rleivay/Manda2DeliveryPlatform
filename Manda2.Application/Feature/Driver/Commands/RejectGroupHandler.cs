// VERSIÓN: v3 — Detección inmediata de ronda agotada
//
// PROPÓSITO: Lógica de negocio al rechazar una oferta de grupo.
//            Operaciones atómicas:
//
//   1. Validar que el motivo existe y está activo (catálogo DriverRejectionReason).
//   2. Validar ReasonNotes si el motivo RequiresNote = true.
//   3. Localizar la notificación activa (Pending) del driver en el intento
//      de dispatch activo (Sent) para el grupo indicado.
//   4. Validar que la oferta sigue vigente.
//   5. Registrar rechazo: Response = Rejected + timestamp + ReasonId/Notes.
//   6. Persistir en una sola transacción (SaveChangesAsync).
//   7. [NUEVO] Evaluar si todos los drivers de la ronda ya respondieron.
//      - Si quedan Pending → retornar. Otros drivers aún pueden aceptar.
//      - Si NO quedan Pending → invocar ExpandDispatchCommand inmediatamente.
//        No esperar el ciclo del DispatchWorker (polling cada N segundos).
//
// ARQUITECTURA:
//   El DispatchWorker sigue siendo el safety net para timeouts/expiraciones.
//   Este handler agrega detección REACTIVA: si el último driver de la ronda
//   rechaza, la siguiente ronda inicia en milisegundos, no en N segundos.
//
// PATRÓN: ICommandHandler<TCommand, TResult> del mediador propio.
// TRANSACCIÓN: SaveChangesAsync antes de invocar ExpandDispatch para
//              garantizar que el rechazo está persistido antes de la expansión.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Handler del comando RejectGroupCommand.
    /// Registra el rechazo explícito de un repartidor con motivo catalogado
    /// y dispara la siguiente ronda de dispatch si la ronda actual se agotó.
    /// </summary>
    public class RejectGroupHandler
        : ICommandHandler<RejectGroupCommand, RejectGroupResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICommandBus _bus;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// ICommandBus es necesario para invocar ExpandDispatchCommand
        /// cuando todos los drivers de la ronda actual han respondido.
        /// </summary>
        public RejectGroupHandler(IApplicationDbContext db, ICommandBus bus)
        {
            _db = db;
            _bus = bus;
        }

        public async Task<RejectGroupResult> HandleAsync(
            RejectGroupCommand command, CancellationToken ct)
        {
            // ────────────────────────────────────────────────────────────────
            // PASO 1: Validar que el motivo existe y está activo.
            //         Evita rechazos con motivos eliminados o desactivados
            //         desde BackOffice entre que la app cargó el catálogo
            //         y el driver presionó "Rechazar".
            // ────────────────────────────────────────────────────────────────
            var reason = await _db.DriverRejectionReasons
                .FirstOrDefaultAsync(r => r.Id == command.ReasonId && r.IsActive, ct);

            if (reason == null)
                return Fail("El motivo de rechazo seleccionado no es válido.");

            // ────────────────────────────────────────────────────────────────
            // PASO 2: Validar ReasonNotes si el motivo lo requiere.
            //         La app MAUI ya lo valida en UI, pero validamos también
            //         en backend para seguridad y consistencia de datos.
            // ────────────────────────────────────────────────────────────────
            if (reason.RequiresNote && string.IsNullOrWhiteSpace(command.ReasonNotes))
                return Fail("Este motivo requiere una descripción adicional.");

            // ────────────────────────────────────────────────────────────────
            // PASO 3: Localizar la notificación activa del driver.
            //
            // Condiciones simultáneas:
            //   - Pertenece al grupo indicado
            //   - Es para este driver
            //   - El driver aún no respondió (Pending)
            //   - El intento de dispatch sigue activo (Sent)
            //
            // Incluimos el DispatchAttempt completo porque en el PASO 7
            // necesitamos su Id para invocar ExpandDispatchCommand.
            // ────────────────────────────────────────────────────────────────
            var notification = await _db.DispatchAttemptDrivers
                .Include(nd => nd.DispatchAttempt)
                .FirstOrDefaultAsync(nd =>
                    nd.DispatchAttempt.OrderGroupId == command.OrderGroupId &&
                    nd.DriverId == command.DriverId &&
                    nd.Response == DriverDispatchResponse.Pending &&
                    nd.DispatchAttempt.Status == DispatchAttemptStatus.Sent,
                    ct);

            // ────────────────────────────────────────────────────────────────
            // PASO 4: Validar que la oferta sigue vigente.
            // ────────────────────────────────────────────────────────────────
            if (notification == null)
                return Fail("La oferta ya no está activa o ya fue procesada.");

            // Capturamos el AttemptId antes de modificar la entidad.
            // Lo necesitamos en el PASO 7 para la consulta de drivers pendientes
            // y para construir el ExpandDispatchCommand.
            int attemptId = notification.DispatchAttemptId;
            int orderGroupId = notification.DispatchAttempt.OrderGroupId;

            // ────────────────────────────────────────────────────────────────
            // PASO 5: Registrar rechazo con motivo y timestamp.
            // ────────────────────────────────────────────────────────────────
            notification.Response = DriverDispatchResponse.Rejected;
            notification.RespondedAtUtc = DateTime.UtcNow;
            notification.ReasonId = command.ReasonId;
            notification.UpdatedAt = DateTime.UtcNow;

            // Solo persistir ReasonNotes si el motivo lo requiere.
            // En otros casos se guarda null explícitamente para limpieza de datos.
            notification.ReasonNotes = reason.RequiresNote
                ? command.ReasonNotes?.Trim()
                : null;

            // ────────────────────────────────────────────────────────────────
            // PASO 6: Persistir el rechazo ANTES de evaluar la ronda.
            //         Garantiza que el estado en BD es consistente antes de
            //         que ExpandDispatchCommand lea los datos del intento.
            // ────────────────────────────────────────────────────────────────
            await _db.SaveChangesAsync(ct);

            // ────────────────────────────────────────────────────────────────
            // PASO 7: Detección inmediata de ronda agotada.
            //
            // Contamos cuántos DispatchAttemptDrivers del mismo intento
            // siguen en Pending. Si el conteo es 0, todos respondieron
            // (Rejected o Expired) y debemos iniciar la siguiente ronda
            // sin esperar el ciclo del DispatchWorker.
            //
            // NOTA: Esta consulta es post-SaveChanges, por lo que el rechazo
            //       que acabamos de persistir ya NO aparece como Pending.
            // ────────────────────────────────────────────────────────────────
            int pendingCount = await _db.DispatchAttemptDrivers
                .CountAsync(d =>
                    d.DispatchAttemptId == attemptId &&
                    d.Response == DriverDispatchResponse.Pending,
                    ct);

            if (pendingCount == 0)
            {
                // Todos los drivers de esta ronda respondieron.
                // Invocar ExpandDispatchCommand de forma reactiva.
                // El handler de Expand se encarga de:
                //   - Verificar MaxRounds → si se alcanzó, pasa a AwaitingManualAssignment.
                //   - Calcular nuevo radio (InitialRadiusKm * RadiusExpansionFactor^ronda).
                //   - Seleccionar candidatos excluyendo rechazos previos.
                //   - Crear nuevo DispatchAttempt + DispatchAttemptDrivers.
                //   - Notificar drivers.
                try
                {
                    var expandResult = await _bus
                        .SendAsync<ExpandDispatchCommand, ExpandDispatchResult>(
                            new ExpandDispatchCommand
                            {
                                OrderGroupId = orderGroupId,
                                PreviousAttemptId = attemptId
                            }, ct);

                    // El resultado de Expand es informativo para logging.
                    // No afecta el resultado del rechazo del driver.
                    // El driver siempre recibe Success = true si su rechazo fue procesado.
                    if (!expandResult.NewAttemptCreated)
                    {
                        // Expand decidió no crear nueva ronda (MaxRounds alcanzado,
                        // sin drivers disponibles, etc.). El grupo pasó a
                        // AwaitingManualAssignment. Solo loguear — no es un error
                        // desde la perspectiva del driver que rechazó.
                        // En producción: reemplazar con ILogger<RejectGroupHandler>.
                        // Por ahora: comportamiento silencioso correcto.
                        _ = expandResult.Message; // Suprimir warning CS0219 si aplica.
                    }
                }
                catch
                {
                    // Si ExpandDispatch falla, el rechazo ya está persistido (PASO 6).
                    // El DispatchWorker actuará como safety net en el próximo ciclo.
                    // No propagamos la excepción para no afectar la respuesta al driver.
                    // En producción: loguear con ILogger<RejectGroupHandler>.
                }
            }

            // ────────────────────────────────────────────────────────────────
            // RETORNO: El driver siempre recibe confirmación de su rechazo,
            //          independientemente de lo que ocurra con la siguiente ronda.
            // ────────────────────────────────────────────────────────────────
            return new RejectGroupResult
            {
                Success = true,
                Message = "Oferta rechazada exitosamente."
            };
        }

        /// <summary>
        /// Helper privado para retornar errores de forma consistente.
        /// </summary>
        private static RejectGroupResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
