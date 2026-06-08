/// <summary>
/// Representa la ruta logística completa de un cliente en un pedido multi-comercio.
/// Es la entidad raíz del dominio de órdenes: agrupa SubOrders (una por comercio),
/// OrderGroupStops (paradas del repartidor) y OrderGroupPayments (pagos split).
///
/// Ciclo de vida del estado:
///   Draft → PendingPayment → PaymentConfirmed → AwaitingDriverAssignment
///   → AssignedToDriver → DriverAccepted → InProgress → Delivered / Cancelled
/// </summary>
/// 
using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class OrderGroup : BaseEntity
    {
        // ─── CLIENTE ────────────────────────────────────────────────────────
        /// <summary>FK al cliente que realizó el pedido.</summary>
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        // ─── DRIVER ─────────────────────────────────────────────────────────
        /// <summary>
        /// FK al repartidor asignado. NULL mientras no haya driver aceptado.
        /// SetNull en BD: si el driver se elimina, el grupo queda sin asignar.
        /// </summary>
        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }

        // ─── ESTADO ─────────────────────────────────────────────────────────
        /// <summary>
        /// Estado actual del grupo en su ciclo de vida.
        /// Guardado como int en SQL (ver OrderGroupStatus enum).
        /// </summary>
        public OrderGroupStatus Status { get; set; } = OrderGroupStatus.Draft;

        // ─── CAPACIDAD / MULTI-LOCAL ─────────────────────────────────────────
        /// <summary>
        /// Cantidad de SubOrders (comercios) en este grupo.
        /// Se usa para validar MaxSubOrderLimit del driver antes del dispatch.
        /// Desnormalizado para evitar COUNT(*) en cada validación.
        /// </summary>
        public int SubOrderCount { get; set; }

        // ─── FINANCIERO CONSOLIDADO ──────────────────────────────────────────
        /// <summary>Cargo de delivery total del grupo (configurable desde cfg.AppConfigs).</summary>
        public decimal DeliveryFee { get; set; }

        /// <summary>Cargo de servicio de plataforma (configurable desde cfg.AppConfigs).</summary>
        public decimal ServiceFee { get; set; }

        /// <summary>
        /// Total a cobrar al cliente: suma de SubOrders + DeliveryFee + ServiceFee.
        /// Debe coincidir con la suma de OrderGroupPayments confirmados para avanzar
        /// al estado PaymentConfirmed.
        /// </summary>
        public decimal TotalAmount { get; set; }

        // ─── DIRECCIÓN DE ENTREGA ────────────────────────────────────────────
        /// <summary>
        /// Texto legible de la dirección de entrega (desnormalizado para el driver).
        /// Ej: "5a Avenida 12-34, Zona 10, Nicaragua".
        /// </summary>
        public string DeliveryAddressText { get; set; } = null!;

        /// <summary>GPS de entrega — decimal(18,10) para precisión milimétrica.</summary>
        public decimal DeliveryLatitude { get; set; }
        public decimal DeliveryLongitude { get; set; }

        // ─── DISPATCH / ASIGNACIÓN DE DRIVER ────────────────────────────────
        /// <summary>
        /// Timestamp exacto en que el driver aceptó el grupo (UTC).
        /// Útil para métricas de tiempo de respuesta y SLA.
        /// </summary>
        public DateTime? DriverAcceptedAtUtc { get; set; }

        /// <summary>Timestamp en que se completó la entrega al cliente (UTC).</summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Posición GPS del driver en el momento de aceptar la oferta.
        /// Permite auditar si el driver estaba realmente cerca al aceptar.
        /// </summary>
        public decimal? DriverLatAtAcceptance { get; set; }
        public decimal? DriverLonAtAcceptance { get; set; }

        // ─── PAGO (DISPLAY / DESNORMALIZADO) ────────────────────────────────
        /// <summary>
        /// Nombre del método de pago principal seleccionado por el cliente.
        /// Desnormalizado para que el driver lo vea sin joins adicionales.
        /// Ej: "Efectivo", "Tarjeta", "Transferencia".
        ///
        /// NOTA: Este campo es de display. El detalle financiero real
        /// (split payment, montos, referencias) vive en OrderGroupPayments.
        /// </summary>
        public string? PaymentMethodName { get; set; }

        /// <summary>
        /// FK al método de pago principal (cfg.PaymentMethods).
        /// Representa la intención de pago del cliente al hacer checkout.
        /// </summary>
        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }

        // ─── COLECCIONES DE NAVEGACIÓN ───────────────────────────────────────

        /// <summary>
        /// SubOrders: un registro por cada comercio incluido en este grupo.
        /// Cascade: se eliminan si el OrderGroup se elimina (solo en Draft).
        /// </summary>
        public virtual ICollection<SubOrder> SubOrders { get; set; } = new List<SubOrder>();

        /// <summary>
        /// Stops: paradas de la ruta del repartidor (Pickup por comercio + Dropoff final).
        /// Cascade: se eliminan con el grupo.
        /// </summary>
        public virtual ICollection<OrderGroupStop> Stops { get; set; } = new List<OrderGroupStop>();

        /// <summary>
        /// DispatchAttempts: historial de rondas de búsqueda de repartidor.
        /// Cascade: se eliminan con el grupo.
        /// </summary>
        public virtual ICollection<DispatchAttempt> DispatchAttempts { get; set; } = new List<DispatchAttempt>();

        /// <summary>
        /// Payments: registros de pago individuales (split payment).
        /// Un grupo puede tener múltiples pagos (Efectivo + Tarjeta, etc.).
        /// La suma de Payments confirmados debe igualar TotalAmount para
        /// avanzar al estado PaymentConfirmed.
        /// Cascade: se eliminan con el grupo (solo aplica en Draft/cancelados).
        /// </summary>
        public virtual ICollection<OrderGroupPayment> Payments { get; set; } = new List<OrderGroupPayment>();
    }
}
