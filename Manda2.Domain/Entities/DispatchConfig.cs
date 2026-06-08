using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class DispatchConfig : BaseEntity
    {
        public string ZoneName { get; set; } = null!;

        // Configuración por Ronda (JSON o Tabla Relacionada)
        // Para el MVP usaremos un Factor de Expansión:
        public decimal InitialRadiusKm { get; set; }     // Ronda 1: 3km
        public decimal RadiusExpansionFactor { get; set; } // +2km por ronda extra

        public int MaxDriversToNotifyPerRound { get; set; }
        public int RoundTimeoutMinutes { get; set; }
        public int MaxRounds { get; set; }
        public bool IsActive { get; set; } = true;

        // ─── Scoring Weights ────────────────────────────────────────────────────────
        /// <summary>Peso del factor fairness en el scoring (0.0 - 1.0). Default: 0.25</summary>
        public decimal FairnessWeight { get; set; } = 0.25m;

        /// <summary>Peso del factor distancia en el scoring (0.0 - 1.0). Default: 0.30</summary>
        public decimal DistanceWeight { get; set; } = 0.30m;

        /// <summary>Peso del factor SLA en el scoring (0.0 - 1.0). Default: 0.25</summary>
        public decimal SlaWeight { get; set; } = 0.25m;

        /// <summary>Peso del factor confiabilidad en el scoring (0.0 - 1.0). Default: 0.20</summary>
        public decimal ReliabilityWeight { get; set; } = 0.20m;

        // ─── Dispatch Timing ────────────────────────────────────────────────────────
        /// <summary>
        /// Minutos antes del vencimiento del SLA en que se activa el modo Rescue.
        /// Si quedan menos de X minutos para el SLA, se ignora fairness y se prioriza velocidad.
        /// Default: 10 minutos.
        /// </summary>
        public int RescueModeThresholdMinutes { get; set; } = 10;

        /// <summary>
        /// Timeout en segundos para rondas en modo urgente/rescue.
        /// Reemplaza RoundTimeoutMinutes cuando la estrategia es Rescue.
        /// Default: 60 segundos.
        /// </summary>
        public int UrgentRoundTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Tolerancia en minutos entre el tiempo de preparación del comercio
        /// y el tiempo estimado de llegada del driver. Si el driver llega antes
        /// de que el pedido esté listo, se penaliza levemente en el score.
        /// Default: 3 minutos.
        /// </summary>
        public int MerchantPrepToleranceMinutes { get; set; } = 3;
    }
}
