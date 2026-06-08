// PROPÓSITO: Vista del estado actual del ciclo de dispatch de un OrderGroup.
//            Consumidores: BackOffice (monitoreo en tiempo real), DispatchWorker.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Dtos
{
    /// <summary>
    /// Estado completo del dispatch de un OrderGroup:
    /// ronda activa + historial de respuestas de drivers.
    /// </summary>
    public class DispatchStatusDto
    {
        public int OrderGroupId { get; set; }

        /// <summary>Estado actual del OrderGroup (ej: "AwaitingDriverAssignment").</summary>
        public string OrderGroupStatus { get; set; } = string.Empty;

        /// <summary>DriverId asignado. Null si aún no hay driver aceptado.</summary>
        public int? AssignedDriverId { get; set; }

        /// <summary>Ronda activa. Null si no hay intento en curso.</summary>
        public ActiveAttemptDto? ActiveAttempt { get; set; }

        /// <summary>Historial de todas las rondas (incluyendo expiradas y aceptadas).</summary>
        public List<AttemptSummaryDto> AttemptHistory { get; set; } = new();
    }

    /// <summary>
    /// Detalle de la ronda activa (Status = Sent).
    /// </summary>
    public class ActiveAttemptDto
    {
        public int AttemptId { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        /// <summary>Segundos restantes antes de que expire la ronda.</summary>
        public int SecondsRemaining { get; set; }

        /// <summary>Drivers notificados en esta ronda con su respuesta actual.</summary>
        public List<DriverOfferStatusDto> NotifiedDrivers { get; set; } = new();
    }

    /// <summary>
    /// Resumen de una ronda pasada (Expired, Accepted, etc.).
    /// </summary>
    public class AttemptSummaryDto
    {
        public int AttemptId { get; set; }
        public int RoundNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        /// <summary>Cantidad de drivers notificados en esta ronda.</summary>
        public int NotifiedCount { get; set; }

        /// <summary>Cantidad de rechazos en esta ronda.</summary>
        public int RejectedCount { get; set; }
    }

    /// <summary>
    /// Estado de la oferta enviada a un driver específico dentro de una ronda.
    /// </summary>
    public class DriverOfferStatusDto
    {
        public int DriverId { get; set; }

        /// <summary>Respuesta: "Pending", "Accepted", "Rejected", "Expired".</summary>
        public string Response { get; set; } = string.Empty;

        public DateTime NotifiedAtUtc { get; set; }
        public DateTime? RespondedAtUtc { get; set; }

        /// <summary>Distancia al primer pickup al momento de notificar (km).</summary>
        public decimal DistanceKm { get; set; }

        /// <summary>ETA estimado al primer pickup en minutos.</summary>
        public int EstimatedArrivalMinutes { get; set; }
    }
}
