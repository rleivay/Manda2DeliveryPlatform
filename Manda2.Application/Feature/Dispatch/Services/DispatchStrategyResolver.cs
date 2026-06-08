using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Services
{
    /// <summary>
    /// Implementación del resolvedor de estrategia de dispatch por ronda.
    ///
    /// Lógica de decisión:
    ///   1. Si el tiempo restante al SLA es menor a RescueModeThresholdMinutes → Rescue
    ///   2. Si es ronda 2 o superior → SlaFirst
    ///   3. Caso base → FairnessFirst
    /// </summary>
    public class DispatchStrategyResolver : IDispatchStrategyResolver
    {
        public DispatchRoundStrategy Resolve(
            int roundNumber,
            DateTime? projectedReadyAtUtc,
            DispatchConfig config)
        {
            // ── Regla 1: Modo Rescue ──────────────────────────────────────────
            // Si el pedido está proyectado a estar listo pronto y ya vamos tarde,
            // activamos modo rescate ignorando fairness.
            if (projectedReadyAtUtc.HasValue)
            {
                double minutesUntilReady = (projectedReadyAtUtc.Value - DateTime.UtcNow).TotalMinutes;

                if (minutesUntilReady <= config.RescueModeThresholdMinutes)
                    return DispatchRoundStrategy.Rescue;
            }

            // ── Regla 2: SLA First desde ronda 2 ─────────────────────────────
            // Si ya fallamos al menos una ronda, priorizamos velocidad sobre fairness.
            if (roundNumber >= 2)
                return DispatchRoundStrategy.SlaFirst;

            // ── Regla 3: Fairness First (ronda 1, sin urgencia) ───────────────
            return DispatchRoundStrategy.FairnessFirst;
        }
    }
}
