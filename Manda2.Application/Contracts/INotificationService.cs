// PROPÓSITO:
//   Contrato (interfaz) del servicio de notificaciones push/real-time.
//   Define los 7 métodos que el módulo Dispatch y Order necesitan para
//   comunicar eventos a Drivers, Clientes y Comercios.
//
// IMPLEMENTACIONES:
//   - NotificationServiceStub   → Manda2.Infrastructure (desarrollo/tests)
//   - SignalRNotificationService → Manda2.Infrastructure (producción D3-C)
//   - FirebaseNotificationService → Manda2.Infrastructure (push móvil, futuro)
//
// PATRÓN:
//   Todos los métodos son async y reciben CancellationToken.
//   No lanzan excepciones de negocio — el caller decide si loguea o reintenta.
//
// INTEGRACIÓN SAP B1:
//   NotifyOrderGroupStatusChangedAsync es el punto de extensión para
//   emitir eventos hacia SAP B1 cuando el grupo alcanza estados financieros
//   (Delivered, Cancelled) en fases posteriores.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Contracts
{
    /// <summary>
    /// Contrato del servicio de notificaciones en tiempo real.
    /// Abstrae el canal de entrega (SignalR, Firebase, SMS, etc.)
    /// del dominio de negocio.
    /// </summary>
    public interface INotificationService
    {
        // ─────────────────────────────────────────────────────────────────
        // MÉTODO 1 — Notificar drivers candidatos al inicio de un dispatch
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Envía notificación a los N drivers candidatos de una ronda de dispatch.
        /// Cada driver recibe su distancia y ETA estimado al primer stop (Pickup).
        /// </summary>
        /// <param name="drivers">
        ///   Lista de tuplas con DriverId, distancia en km y ETA en minutos.
        ///   Ordenada por distancia ascendente (más cercano primero).
        /// </param>
        /// <param name="orderGroupId">ID del OrderGroup a asignar.</param>
        /// <param name="attemptId">ID del DispatchAttempt de esta ronda.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task NotifyDriversAsync(
            IEnumerable<(int DriverId, double DistanceKm, int EtaMinutes)> drivers,
            int orderGroupId,
            int attemptId,
            CancellationToken cancellationToken = default);

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO 2 — Driver aceptó la orden
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Notifica al Cliente y al Comercio que un driver aceptó el pedido.
        /// También notifica al resto de drivers de la ronda que ya no aplica.
        /// </summary>
        /// <param name="orderGroupId">ID del OrderGroup asignado.</param>
        /// <param name="driverId">ID del driver que aceptó.</param>
        /// <param name="customerId">ID del cliente dueño del grupo.</param>
        /// <param name="merchantIds">IDs de los comercios involucrados en el grupo.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task NotifyDriverAcceptedAsync(
            int orderGroupId,
            int driverId,
            int customerId,
            IEnumerable<int> merchantIds,
            CancellationToken cancellationToken = default);

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO 3 — Driver rechazó la orden
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Registra internamente que un driver rechazó o ignoró la notificación.
        /// No notifica al cliente — el sistema reintenta con otro driver.
        /// </summary>
        /// <param name="attemptId">ID del DispatchAttempt de la ronda.</param>
        /// <param name="driverId">ID del driver que rechazó.</param>
        /// <param name="reason">Motivo de rechazo (del catálogo DriverRejectionReason).</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task NotifyDriverRejectedAsync(
            int attemptId,
            int driverId,
            string? reason,
            CancellationToken cancellationToken = default);

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO 4 — Cambio de estado del OrderGroup
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Notifica al Cliente y Comercios cuando el OrderGroup cambia de estado.
        /// Es el punto de extensión para integración futura con SAP B1
        /// en estados financieros (Delivered, Cancelled).
        /// </summary>
        /// <param name="orderGroupId">ID del OrderGroup.</param>
        /// <param name="newStatus">Nuevo estado del grupo (OrderGroupStatus).</param>
        /// <param name="customerId">ID del cliente a notificar.</param>
        /// <param name="merchantIds">IDs de comercios a notificar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task NotifyOrderGroupStatusChangedAsync(
            int orderGroupId,
            string newStatus,
            int customerId,
            IEnumerable<int> merchantIds,
            CancellationToken cancellationToken = default);

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO 5 — Driver llegó a un stop
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Notifica cuando el driver llega a un stop de la ruta (Pickup o Dropoff).
        /// - Pickup  → notifica al Comercio para preparar entrega al driver.
        /// - Dropoff → notifica al Cliente que el driver está en su puerta.
        /// </summary>
        /// <param name="orderGroupId">ID del OrderGroup.</param>
        /// <param name="stopId">ID del OrderGroupStop al que llegó.</param>
        /// <param name="stopType">"Pickup" o "Dropoff".</param>
        /// <param name="targetId">
        ///   ID del actor a notificar:
        ///   - Pickup  → MerchantId
        ///   - Dropoff → CustomerId
        /// </param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task NotifyDriverArrivedAtStopAsync(
            int orderGroupId,
            int stopId,
            string stopType,
            int targetId,
            CancellationToken cancellationToken = default);

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO 6 — Actualización de ubicación GPS del driver
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Transmite la posición GPS actualizada del driver al cliente en tiempo real.
        /// Alta frecuencia (cada 5-10 segundos) — implementación debe ser liviana.
        /// En producción usa SignalR Hub directo, no cola de mensajes.
        /// </summary>
        /// <param name="driverId">ID del driver.</param>
        /// <param name="orderGroupId">ID del OrderGroup activo del driver.</param>
        /// <param name="latitude">Latitud actual.</param>
        /// <param name="longitude">Longitud actual.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task NotifyDriverLocationUpdatedAsync(
            int driverId,
            int orderGroupId,
            decimal latitude,
            decimal longitude,
            CancellationToken cancellationToken = default);

        // ─────────────────────────────────────────────────────────────────
        // MÉTODO 7 — Ronda de dispatch expiró sin aceptación
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Notifica internamente (Backoffice/operador) que una ronda de dispatch
        /// expiró sin que ningún driver aceptara.
        /// El sistema puede iniciar una nueva ronda con radio expandido
        /// o escalar a operador manual según DispatchConfig.
        /// </summary>
        /// <param name="attemptId">ID del DispatchAttempt expirado.</param>
        /// <param name="orderGroupId">ID del OrderGroup sin asignar.</param>
        /// <param name="roundNumber">Número de ronda (1, 2, 3...) para escalamiento.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task NotifyDispatchExpiredAsync(
            int attemptId,
            int orderGroupId,
            int roundNumber,
            CancellationToken cancellationToken = default);

        Task NotifyDriverDispatchOfferAsync(
    int driverId,
    int attemptId,
    int orderGroupId,
    CancellationToken cancellationToken = default);

    }
}
