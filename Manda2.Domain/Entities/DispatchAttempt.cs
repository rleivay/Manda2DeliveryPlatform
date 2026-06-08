using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Domain.Entities
{
    public class DispatchAttempt : BaseEntity
    {
        public int OrderGroupId { get; set; }
        public virtual OrderGroup OrderGroup { get; set; } = null!;

        public int RoundNumber { get; set; } // Ronda 1, 2, 3...
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DispatchEnums.DispatchAttemptStatus Status { get; set; }

        /// <summary>
        /// Estrategia de dispatch usada en esta ronda.
        /// Determinada por DispatchStrategyResolver según el contexto operacional.
        /// </summary>
        public DispatchRoundStrategy Strategy { get; set; } = DispatchRoundStrategy.FairnessFirst;

        /// <summary>
        /// Proyección de cuándo estará listo el pedido en el comercio.
        /// Calculado por PrepTimeEngine al iniciar el dispatch.
        /// Null si no se pudo calcular (pedido sin productos con prep time).
        /// </summary>
        public DateTime? ProjectedReadyAtUtc { get; set; }

        public virtual ICollection<DispatchAttemptDriver> NotifiedDrivers { get; set; } = new List<DispatchAttemptDriver>();
    }
}
