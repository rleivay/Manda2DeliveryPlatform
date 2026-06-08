using Manda2.Application.Feature.Dispatch.Dtos;
using Manda2.Domain.Entities;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Services
{
    /// <summary>
    /// Motor de scoring compuesto para selección de drivers.
    /// 
    /// Fórmula base (FairnessFirst):
    ///   Score = (DistanceScore * DistanceWeight)
    ///         + (SlaScore      * SlaWeight)
    ///         + (FairnessScore * FairnessWeight)
    ///         + (ReliabilityScore * ReliabilityWeight)
    ///
    /// En modo SlaFirst: FairnessWeight se redistribuye a SLA y Distancia.
    /// En modo Rescue:   Solo SLA + Distancia. Fairness = 0.
    /// </summary>
    public class DispatchScoringService : IDispatchScoringService
    {
        public List<DispatchCandidateDto> ScoreAndRank(
            List<DispatchCandidateDto> candidates,
            DispatchConfig config,
            DispatchRoundStrategy strategy)
        {
            if (candidates == null || candidates.Count == 0)
                return new List<DispatchCandidateDto>();

            // ── Paso 1: Normalizar métricas (0.0 - 1.0) ──────────────────────
            decimal maxDistance = candidates.Max(c => c.DistanceKm);
            int maxArrival = candidates.Max(c => c.EstimatedArrivalMinutes);
            int maxActiveOrders = candidates.Max(c => c.ActiveOrderCount);

            // ── Paso 2: Calcular pesos según estrategia ───────────────────────
            decimal wDistance = config.DistanceWeight;
            decimal wSla = config.SlaWeight;
            decimal wFairness = config.FairnessWeight;
            decimal wReliability = config.ReliabilityWeight;

            switch (strategy)
            {
                case DispatchRoundStrategy.SlaFirst:
                    // Redistribuimos el peso de fairness a SLA y distancia
                    wSla += wFairness * 0.6m;
                    wDistance += wFairness * 0.4m;
                    wFairness = 0m;
                    break;

                case DispatchRoundStrategy.Rescue:
                    // Solo velocidad importa — ignoramos fairness y confiabilidad
                    wSla += (wFairness + wReliability) * 0.5m;
                    wDistance += (wFairness + wReliability) * 0.5m;
                    wFairness = 0m;
                    wReliability = 0m;
                    break;

                    // FairnessFirst: pesos estándar de DispatchConfig
            }

            // ── Paso 3: Calcular score por candidato ──────────────────────────
            foreach (var candidate in candidates)
            {
                // Distancia: menor distancia = mayor score
                decimal distanceScore = maxDistance > 0
                    ? 1m - (candidate.DistanceKm / maxDistance)
                    : 1m;

                // SLA: penalizamos si el driver llega ANTES del pedido listo
                // PrepTimeDeltaMinutes positivo = llega después (ideal)
                // Normalizamos: delta entre -30 y +30 → 0.0 a 1.0
                decimal slaScore = Math.Clamp(
                    (candidate.PrepTimeDeltaMinutes + 30m) / 60m,
                    0m, 1m);

                // Fairness: menos pedidos activos = mayor score
                decimal fairnessScore = maxActiveOrders > 0
                    ? 1m - ((decimal)candidate.ActiveOrderCount / maxActiveOrders)
                    : 1m;

                // Confiabilidad: AcceptanceRate ya está en 0.0 - 1.0
                decimal reliabilityScore = candidate.AcceptanceRate;

                // Score compuesto
                candidate.CompositeScore =
                    (distanceScore * wDistance) +
                    (slaScore * wSla) +
                    (fairnessScore * wFairness) +
                    (reliabilityScore * wReliability);

                // Desglose para auditoría
                candidate.ScoreBreakdown =
                    $"Distance:{distanceScore:F2}*{wDistance:F2}" +
                    $"|SLA:{slaScore:F2}*{wSla:F2}" +
                    $"|Fairness:{fairnessScore:F2}*{wFairness:F2}" +
                    $"|Reliability:{reliabilityScore:F2}*{wReliability:F2}" +
                    $"|Total:{candidate.CompositeScore:F4}";
            }

            // ── Paso 4: Ordenar de mayor a menor score ────────────────────────
            return candidates.OrderByDescending(c => c.CompositeScore).ToList();
        }
    }
}
