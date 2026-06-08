// PROPÓSITO:
//   Handler que procesa el completado de una parada (OrderGroupStop) en la ruta
//   del repartidor. Es invocado desde DriverController cuando el driver confirma:
//     • Que llegó y recogió el pedido en un comercio  → StopType.Pickup
//     • Que entregó el pedido al cliente              → StopType.Dropoff
//
// PATRÓN:
//   Implementa ICommandHandler<TCommand, TResult> del mediador propio Manda2.
//   NO usa MediatR. El bus de comandos (ICommandBus) resuelve este handler
//   automáticamente por DI gracias a MediatorExtensions.AddManda2Mediator().
//
// REGLAS DE NEGOCIO:
//   1. Idempotencia     → Si la parada ya fue completada, retorna resultado sin error.
//   2. Seguridad        → Solo el driver asignado al OrderGroup puede completar paradas.
//   3. Pickup completo  → Marca SubOrder del comercio como PickedUp.
//                         Si todos los Pickups están completos → OrderGroup pasa a InRoute.
//   4. Dropoff completo → Marca SubOrders en PickedUp como Delivered.
//                         Si todos los Dropoffs están completos → OrderGroup pasa a Delivered
//                         y el driver queda liberado (Status=Available, CurrentOrderGroupId=null).
//   5. AuditLog         → Toda acción queda registrada en aud.AuditLogs (inmutable).
//   6. Notificaciones   → Delegadas a INotificationService (stub en MVP, SignalR en D3-C).
//
// TRANSICIONES DE ESTADO DEL OrderGroup:
//   DriverAccepted → [último Pickup completado]  → InRoute
//   InRoute        → [último Dropoff completado] → Delivered
//
// EXTENSIBILIDAD:
//   • Multi-Dropoff: la lógica de Dropoff ya soporta múltiples paradas de entrega.
//     En MVP hay un solo Dropoff, pero el código no asume eso.
//   • INotificationService: inyectar cuando esté implementado (D3-C).
//     Por ahora se inyecta el stub registrado en DispatchServiceRegistration.
//
// DEPENDENCIAS:
//   • IApplicationDbContext  → acceso a BD (EF Core)
//   • INotificationService   → notificaciones push/SignalR (stub en MVP)
// ═══════════════════════════════════════════════════════════════════════════
using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Handler para <see cref="CompleteStopCommand"/>.
    /// Gestiona el ciclo de vida de las paradas de la ruta del repartidor,
    /// incluyendo transiciones de estado del OrderGroup y liberación del driver.
    /// </summary>
    public class CompleteStopCommandHandler : ICommandHandler<CompleteStopCommand, CompleteStopResult>
    {
        // ─── DEPENDENCIAS ────────────────────────────────────────────────────────
        private readonly IApplicationDbContext _db;
        private readonly INotificationService _notifications;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// INotificationService se resuelve como NotificationServiceStub en MVP.
        /// Reemplazar el registro en DispatchServiceRegistration cuando SignalR esté listo.
        /// </summary>
        public CompleteStopCommandHandler(
            IApplicationDbContext db,
            INotificationService notifications)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        }

        // ─── HANDLER PRINCIPAL ───────────────────────────────────────────────────
        /// <summary>
        /// Punto de entrada del handler. Orquesta todas las reglas de negocio
        /// para completar una parada de forma segura y consistente.
        /// </summary>
        public async Task<CompleteStopResult> HandleAsync(
            CompleteStopCommand request,
            CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Cargar Stop con su contexto completo ─────────────────────
            // Necesitamos el OrderGroup, todos sus Stops (para evaluar transiciones)
            // y todas sus SubOrders (para actualizar estados por comercio).
            // Un solo query con Include evita N+1 queries.
            var stop = await _db.OrderGroupStops
                .Include(s => s.OrderGroup!)
                    .ThenInclude(og => og.SubOrders!)
                .Include(s => s.OrderGroup)
                    .ThenInclude(og => og.Stops)
                .FirstOrDefaultAsync(s => s.Id == request.StopId, cancellationToken);

            if (stop == null)
                throw new InvalidOperationException(
                    $"Parada {request.StopId} no encontrada.");

            var group = stop.OrderGroup
                ?? throw new InvalidOperationException(
                    $"La parada {request.StopId} no tiene OrderGroup asociado. " +
                    "Esto indica un problema de integridad en la base de datos.");

            // ── PASO 2: Validar que el driver es el asignado al grupo ────────────
            // Seguridad crítica: un driver no puede completar paradas de otro grupo.
            // El DriverId viene del JWT en el controller, no del body del request.
            if (group.DriverId != request.DriverId)
                throw new UnauthorizedAccessException(
                    $"El repartidor {request.DriverId} no está asignado al grupo {group.Id}. " +
                    $"Driver asignado: {group.DriverId?.ToString() ?? "ninguno"}.");

            // ── PASO 3: Idempotencia ─────────────────────────────────────────────
            // Si la parada ya fue completada (ej: doble tap en la app del driver),
            // retornamos el resultado sin error ni efecto secundario.
            // Esto protege contra reintentos de red y doble submit.
            if (stop.CompletedAt.HasValue)
            {
                return new CompleteStopResult
                {
                    StopId = stop.Id,
                    StopType = stop.StopType.ToString(),
                    OrderGroupId = group.Id,
                    OrderGroupCompleted = group.SubOrders != null &&
                                         group.SubOrders.All(so => so.Status == SubOrderStatus.Delivered),
                    Message = $"La parada {stop.Id} ya fue completada el {stop.CompletedAt:u}. " +
                              "No se realizaron cambios (idempotencia)."
                };
            }

            // ── PASO 4: Marcar la parada como completada ─────────────────────────
            stop.CompletedAt = utcNow;
            stop.ArrivedAt = stop.ArrivedAt ?? utcNow; // Si no se registró llegada, usar ahora

            // Guardar nota del driver si viene en el request
            // Ejemplo: "Cliente ausente, dejé con el portero del edificio"
            if (!string.IsNullOrWhiteSpace(request.Notes))
                stop.Notes = request.Notes;

            // ── PASO 5: Aplicar lógica de negocio según tipo de parada ───────────
            bool groupCompleted = false;

            if (stop.StopType == StopType.Pickup)
            {
                // ── 5A: PICKUP — Recolección en comercio ─────────────────────────
                // Buscar la SubOrder que corresponde al comercio de esta parada.
                // La relación es: OrderGroupStop.MerchantId = SubOrder.MerchantId
                var subOrder = group.SubOrders?
                    .FirstOrDefault(so => so.MerchantId == stop.MerchantId);

                if (subOrder == null)
                    throw new InvalidOperationException(
                        $"No se encontró SubOrder para el comercio {stop.MerchantId} " +
                        $"en el grupo {group.Id}. Verifique la integridad del pedido.");

                // Avanzar estado de la SubOrder: en camino → recogido
                subOrder.Status = SubOrderStatus.PickedUp;
                subOrder.PickedUpAt = utcNow;

                // ── Evaluar transición del OrderGroup ────────────────────────────
                // Si ya no quedan Pickups pendientes (excluyendo el que acabamos de completar),
                // el driver ya recogió en todos los comercios → puede ir hacia el cliente.
                //
                // NOTA: stop.CompletedAt ya fue seteado arriba, pero EF aún no guardó.
                // Por eso excluimos el stop actual por Id para no contarlo como pendiente.
                var pickupsPendientes = group.Stops
                    .Count(s => s.StopType == StopType.Pickup
                             && !s.CompletedAt.HasValue
                             && s.Id != stop.Id);

                if (pickupsPendientes == 0)
                {
                    // Todos los comercios fueron visitados → driver en ruta al cliente
                    group.Status = OrderGroupStatus.InRoute;
                }
                // Si aún hay pickups pendientes, el grupo permanece en DriverAccepted.
                // No se cambia el estado — el driver sigue visitando comercios.
            }
            else // StopType.Dropoff
            {
                // ── 5B: DROPOFF — Entrega al cliente ─────────────────────────────
                // Marcar todas las SubOrders que estaban en PickedUp como Delivered.
                // En MVP hay un solo Dropoff, pero el código soporta múltiples
                // (ej: entrega parcial en edificios con varios apartamentos).
                if (group.SubOrders != null)
                {
                    foreach (var so in group.SubOrders.Where(s => s.Status == SubOrderStatus.PickedUp))
                    {
                        so.Status = SubOrderStatus.Delivered;
                        so.DeliveredAt = utcNow;
                    }

                    // El grupo está completo cuando TODAS las SubOrders están entregadas.
                    // Esto cubre el caso multi-Dropoff: si hay 2 dropoffs y solo se completó 1,
                    // puede quedar alguna SubOrder aún en PickedUp.
                    groupCompleted = group.SubOrders.All(so => so.Status == SubOrderStatus.Delivered);
                }

                if (groupCompleted)
                {
                    // ── Cerrar el OrderGroup ─────────────────────────────────────
                    group.Status = OrderGroupStatus.Delivered;
                    group.DeliveredAt = utcNow;

                    // ── Liberar al driver ────────────────────────────────────────
                    // El driver queda disponible para recibir nuevos pedidos.
                    // CurrentOrderGroupId = null → elegible para el motor de dispatch.
                    var driver = await _db.Drivers
                        .FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken);

                    if (driver != null)
                    {
                        driver.CurrentOrderGroupId = null;
                        driver.Status = DriverStatus.Available;
                        // Nota: IsOnline no se toca — el driver decide si desconectarse.
                    }
                }
            }

            // ── PASO 6: Registrar en AuditLog ────────────────────────────────────
            // El AuditLog es inmutable: solo INSERT, nunca UPDATE.
            // Permite trazabilidad completa para soporte, forensics y SAP B1.
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroupStop),
                EntityId = stop.Id,
                Action = $"CompleteStop_{stop.StopType}",
                PerformedByUserId = request.DriverId,
                Details = groupCompleted
                    ? $"Grupo {group.Id} completado. Driver {request.DriverId} liberado. " +
                      $"Parada {stop.Id} ({stop.StopType}) completada."
                    : $"Parada {stop.Id} ({stop.StopType}) completada. " +
                      $"Grupo {group.Id} → {group.Status}. En progreso.",
                CreatedAt = utcNow
            });

            // ── PASO 7: Persistir todos los cambios en una sola transacción ──────
            // EF Core agrupa todos los cambios del contexto en un solo SaveChanges.
            // Si algo falla, el rollback es automático (Unit of Work pattern).
            await _db.SaveChangesAsync(cancellationToken);

            // ── PASO 8: Notificar al actor correspondiente ────────────────────────
            // Se ejecuta DESPUÉS del SaveChanges para garantizar que la BD
            // ya tiene el estado correcto antes de notificar.
            // Si la notificación falla, el estado en BD ya está correcto (no se revierte).
            await NotifyStopCompletedAsync(stop, group, groupCompleted, cancellationToken);

            // ── PASO 9: Retornar resultado al controller ──────────────────────────
            return new CompleteStopResult
            {
                StopId = stop.Id,
                StopType = stop.StopType.ToString(),
                OrderGroupId = group.Id,
                OrderGroupCompleted = groupCompleted,
                Message = groupCompleted
                    ? "Entrega completada. Repartidor liberado y disponible para nuevos pedidos."
                    : $"Parada {stop.StopType} completada exitosamente. " +
                      $"Grupo {group.Id} → {group.Status}."
            };
        }

        // ─── MÉTODOS PRIVADOS ────────────────────────────────────────────────────

        /// <summary>
        /// Envía notificaciones push/SignalR al actor correspondiente según el tipo de parada:
        /// <list type="bullet">
        ///   <item><description>Pickup  → Notifica al Comercio que el driver llegó a recoger.</description></item>
        ///   <item><description>Dropoff → Notifica al Cliente que el driver llegó / entregó.</description></item>
        ///   <item><description>Delivered → Notifica a todos los actores del grupo (cliente + comercios).</description></item>
        /// </list>
        /// En MVP usa <see cref="NotificationServiceStub"/> (no-op).
        /// En D3-C se reemplaza por la implementación real con SignalR + Firebase.
        /// </summary>
        private async Task NotifyStopCompletedAsync(
            OrderGroupStop stop,
            OrderGroup group,
            bool isGroupCompleted,
            CancellationToken cancellationToken)
        {
            if (stop.StopType == StopType.Pickup && stop.MerchantId.HasValue)
            {
                // Notificar al comercio: "El repartidor llegó a recoger tu pedido"
                await _notifications.NotifyDriverArrivedAtStopAsync(
                    orderGroupId: group.Id,
                    stopId: stop.Id,
                    stopType: "Pickup",
                    targetId: stop.MerchantId.Value,
                    cancellationToken: cancellationToken);
            }
            else if (stop.StopType == StopType.Dropoff)
            {
                // Notificar al cliente: "Tu pedido fue entregado" o "El driver llegó"
                await _notifications.NotifyDriverArrivedAtStopAsync(
                    orderGroupId: group.Id,
                    stopId: stop.Id,
                    stopType: isGroupCompleted ? "Delivered" : "Dropoff",
                    targetId: group.CustomerId,
                    cancellationToken: cancellationToken);

                // Si el grupo quedó completado, notificar cambio de estado global
                // a todos los actores: cliente + todos los comercios del grupo.
                if (isGroupCompleted && group.SubOrders != null)
                {
                    var merchantIds = group.SubOrders
                        .Select(so => so.MerchantId)
                        .Distinct();

                    await _notifications.NotifyOrderGroupStatusChangedAsync(
                        orderGroupId: group.Id,
                        newStatus: OrderGroupStatus.Delivered.ToString(),
                        customerId: group.CustomerId,
                        merchantIds: merchantIds,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}
