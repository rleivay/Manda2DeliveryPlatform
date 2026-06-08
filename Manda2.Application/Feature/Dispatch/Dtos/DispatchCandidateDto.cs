using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Dtos
{
    /// <summary>
    /// DTO interno del motor de dispatch. Representa un driver candidato
    /// enriquecido con métricas de scoring calculadas por DispatchScoringService.
    /// NO se expone en la API — es un objeto de trabajo interno entre servicios.
    /// </summary>
    public class DispatchCandidateDto
    {
        // ─── Identidad del Driver ────────────────────────────────────────────
        /// <summary>ID del driver candidato.</summary>
        public int DriverId { get; set; }

        /// <summary>Nombre completo del driver (para logs y notificaciones).</summary>
        public string FullName { get; set; } = string.Empty;

        // ─── Métricas de Distancia ───────────────────────────────────────────
        /// <summary>
        /// Distancia en km desde la posición actual del driver
        /// hasta el primer punto de pickup del OrderGroup.
        /// Calculado con Haversine en StartDispatchCommandHandler.
        /// </summary>
        public decimal DistanceKm { get; set; }

        /// <summary>
        /// Tiempo estimado de llegada al primer pickup (en minutos).
        /// Calculado con base en DistanceKm y velocidad promedio de la zona.
        /// </summary>
        public int EstimatedArrivalMinutes { get; set; }

        // ─── Métricas de SLA ─────────────────────────────────────────────────
        /// <summary>
        /// Diferencia en minutos entre el tiempo estimado de llegada del driver
        /// y el tiempo proyectado de preparación del pedido (ProjectedReadyAtUtc).
        /// Positivo = driver llega después de que el pedido está listo (ideal).
        /// Negativo = driver llega antes (espera en comercio, penalización leve).
        /// </summary>
        public int PrepTimeDeltaMinutes { get; set; }

        // ─── Métricas de Fairness ────────────────────────────────────────────
        /// <summary>
        /// Cantidad de pedidos activos que tiene el driver en este momento.
        /// Drivers con menos pedidos activos reciben mayor puntaje de fairness.
        /// </summary>
        public int ActiveOrderCount { get; set; }

        // ─── Métricas de Confiabilidad ───────────────────────────────────────
        /// <summary>
        /// Tasa de aceptación histórica del driver (0.0 - 1.0).
        /// 1.0 = acepta siempre. 0.0 = rechaza siempre.
        /// Fuente: calculado externamente antes de construir este DTO.
        /// </summary>
        public decimal AcceptanceRate { get; set; }

        // ─── Score Final ─────────────────────────────────────────────────────
        /// <summary>
        /// Score compuesto calculado por DispatchScoringService.
        /// Rango: 0.0 - 1.0. Mayor score = mejor candidato.
        /// </summary>
        public decimal CompositeScore { get; set; }

        /// <summary>
        /// Desglose del score por factor (para logging y auditoría).
        /// Formato: "Distance:0.30|SLA:0.25|Fairness:0.25|Reliability:0.20"
        /// </summary>
        public string ScoreBreakdown { get; set; } = string.Empty;
    }
}
