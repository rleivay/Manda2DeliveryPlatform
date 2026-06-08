using Manda2.Application.Feature.Dispatch.Dtos;
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
    /// Contrato del motor de scoring de candidatos para dispatch.
    /// Recibe una lista de candidatos crudos y retorna la misma lista
    /// con CompositeScore y ScoreBreakdown calculados, ordenada de mayor a menor score.
    /// </summary>
    public interface IDispatchScoringService
    {
        /// <summary>
        /// Calcula y ordena los candidatos según la estrategia activa.
        /// </summary>
        /// <param name="candidates">Lista de candidatos con métricas ya cargadas.</param>
        /// <param name="config">Configuración de dispatch de la zona (pesos de scoring).</param>
        /// <param name="strategy">Estrategia de la ronda actual.</param>
        /// <returns>Lista ordenada de mayor a menor score.</returns>
        List<DispatchCandidateDto> ScoreAndRank(
            List<DispatchCandidateDto> candidates,
            DispatchConfig config,
            DispatchRoundStrategy strategy);
    }
}
