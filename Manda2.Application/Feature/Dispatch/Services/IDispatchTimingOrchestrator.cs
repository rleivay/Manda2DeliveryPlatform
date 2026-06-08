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
    /// Contrato del orquestador de timing de dispatch.
    /// Calcula cuándo expira una ronda según la estrategia activa.
    /// </summary>
    public interface IDispatchTimingOrchestrator
    {
        /// <summary>
        /// Calcula la fecha/hora de expiración (UTC) de la ronda actual.
        /// </summary>
        /// <param name="strategy">Estrategia activa en esta ronda.</param>
        /// <param name="config">Configuración de dispatch de la zona.</param>
        /// <returns>DateTime UTC en que expira la ronda.</returns>
        DateTime CalculateRoundExpiry(DispatchRoundStrategy strategy, DispatchConfig config);

        /// <summary>
        /// Determina si una ronda ya expiró.
        /// </summary>
        bool IsExpired(DateTime expiresAt);
    }
}
