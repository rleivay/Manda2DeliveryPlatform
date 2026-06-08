// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Domain/Entities/DispatchAttemptDriver.cs
//
// PROPÓSITO: Registro de cada notificación enviada a un repartidor
//            dentro de un intento de dispatch.
//
// CAMBIOS v2:
//   + ReasonId: FK al catálogo DriverRejectionReason (nullable, solo en rechazo)
//   + ReasonNotes: texto libre cuando el motivo es "Otro motivo"
//   + Navegación a DriverRejectionReason
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Domain.Common;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Domain.Entities
{
    /// <summary>
    /// Representa la notificación de una oferta de pedido a un repartidor específico
    /// dentro de un intento de dispatch. Registra la respuesta y, si aplica, el motivo
    /// de rechazo con su nota libre.
    /// </summary>
    public class DispatchAttemptDriver : BaseEntity
    {
        // ─── RELACIONES PRINCIPALES ───────────────────────────────────────

        /// <summary>FK al intento de dispatch al que pertenece esta notificación.</summary>
        public int DispatchAttemptId { get; set; }

        /// <summary>Navegación hacia el intento de dispatch.</summary>
        public virtual DispatchAttempt DispatchAttempt { get; set; } = null!;

        /// <summary>FK hacia el repartidor notificado.</summary>
        public int DriverId { get; set; }

        /// <summary>Navegación hacia el repartidor.</summary>
        public virtual Driver Driver { get; set; } = null!;

        // ─── RESPUESTA DEL DRIVER ─────────────────────────────────────────

        /// <summary>
        /// Estado de la respuesta del driver ante esta notificación.
        /// Pending → Accepted | Rejected | Expired
        /// </summary>
        public DriverDispatchResponse Response { get; set; } = DriverDispatchResponse.Pending;

        /// <summary>Momento en que el driver fue notificado.</summary>
        public DateTime NotifiedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Momento en que el driver respondió (aceptó, rechazó o expiró).
        /// Null si aún está Pending.
        /// </summary>
        public DateTime? RespondedAtUtc { get; set; }

        // ─── MOTIVO DE RECHAZO ────────────────────────────────────────────

        /// <summary>
        /// FK al catálogo de motivos de rechazo.
        /// Null si el driver aceptó o si la oferta expiró sin respuesta.
        /// Solo se popula cuando Response = Rejected.
        /// </summary>
        public int? ReasonId { get; set; }

        /// <summary>
        /// Navegación al catálogo de motivos de rechazo.
        /// </summary>
        public virtual DriverRejectionReason? RejectionReason { get; set; }

        /// <summary>
        /// Texto libre del driver cuando selecciona el motivo "Otro motivo"
        /// (DriverRejectionReason.RequiresNote = true).
        /// Null en todos los demás casos.
        /// Max 500 caracteres para evitar abuso.
        /// </summary>
        public string? ReasonNotes { get; set; }

        // ─── LOGÍSTICA ────────────────────────────────────────────────────

        /// <summary>Distancia estimada al punto inicial de recogida al momento de notificar.</summary>
        public decimal DistanceKm { get; set; }

        /// <summary>Tiempo estimado de llegada al primer pickup en minutos.</summary>
        public int EstimatedArrivalMinutes { get; set; }
    }
}