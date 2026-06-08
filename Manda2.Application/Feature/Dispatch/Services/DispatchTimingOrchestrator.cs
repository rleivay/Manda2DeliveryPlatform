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
    /// Implementación del orquestador de timing de rondas de dispatch.
    ///
    /// Reglas de timeout:
    ///   - FairnessFirst → RoundTimeoutMinutes (configurado en DispatchConfig)
    ///   - SlaFirst      → RoundTimeoutMinutes (mismo, pero con scoring diferente)
    ///   - Rescue        → UrgentRoundTimeoutSeconds (timeout reducido, modo urgente)
    /// </summary>
    public class DispatchTimingOrchestrator : IDispatchTimingOrchestrator
    {
        public DateTime CalculateRoundExpiry(DispatchRoundStrategy strategy, DispatchConfig config)
        {
            var now = DateTime.UtcNow;

            return strategy switch
            {
                // Modo rescate: timeout en segundos (mucho más corto)
                DispatchRoundStrategy.Rescue =>
                    now.AddSeconds(config.UrgentRoundTimeoutSeconds),

                // Modos normales: timeout en minutos
                _ => now.AddMinutes(config.RoundTimeoutMinutes)
            };
        }

        public bool IsExpired(DateTime expiresAt)
        {
            // Comparamos en UTC para evitar problemas de zona horaria
            return DateTime.UtcNow >= expiresAt;
        }
    }
}
