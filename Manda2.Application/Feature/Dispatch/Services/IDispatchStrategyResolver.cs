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
    /// Contrato del resolvedor de estrategia por ronda.
    /// Determina qué estrategia de scoring aplicar según el contexto operacional.
    /// </summary>
    public interface IDispatchStrategyResolver
    {
        /// <summary>
        /// Determina la estrategia óptima para la ronda actual.
        /// </summary>
        /// <param name="roundNumber">Número de ronda actual (1-based).</param>
        /// <param name="projectedReadyAtUtc">Cuándo estará listo el pedido (UTC).</param>
        /// <param name="config">Configuración de dispatch de la zona.</param>
        /// <returns>Estrategia a aplicar en esta ronda.</returns>
        DispatchRoundStrategy Resolve(
            int roundNumber,
            DateTime? projectedReadyAtUtc,
            DispatchConfig config);
    }
}
