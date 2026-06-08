// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: GetAvailableDriversForDispatchQuery.cs
//
// PROPÓSITO: Consulta los repartidores elegibles para recibir una oferta
//            de un OrderGroup específico.
//
// CRITERIOS DE ELEGIBILIDAD:
//   1. IsActive = true         → Repartidor habilitado en la plataforma
//   2. IsOnline = true         → Repartidor conectado y disponible
//   3. IsCashBlocked = false   → Sin deuda pendiente de liquidación
//   4. CurrentOrderGroupId = null → Sin grupo activo asignado
//   5. GPS reciente            → LastLocationUpdateAt dentro del umbral configurable
//   6. Dentro del radio        → Distancia al primer pickup <= radio de la ronda actual
//
// PATRÓN: IQuery<T> / IQueryHandler — Mediador propio Manda2.Application.Mediator
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.Dispatch.Dtos;
using Manda2.Application.Mediator;

namespace Manda2.Application.Feature.Dispatch.Queries
{
    public class GetAvailableDriversForDispatchQuery : IQuery<List<AvailableDriverDto>>
    {
        /// <summary>
        /// Latitud del primer punto de recogida (Merchant) del OrderGroup.
        /// Es el punto de referencia para calcular distancia al driver.
        /// </summary>
        public decimal PickupLatitude { get; set; }

        /// <summary>
        /// Longitud del primer punto de recogida (Merchant) del OrderGroup.
        /// </summary>
        public decimal PickupLongitude { get; set; }

        /// <summary>
        /// Radio en kilómetros dentro del cual buscar drivers.
        /// Calculado por el motor de dispatch según la ronda actual:
        /// Ronda N = InitialRadiusKm + ((N-1) × RadiusExpansionFactor)
        /// </summary>
        public decimal RadiusKm { get; set; }

        /// <summary>
        /// Límite de minutos para considerar una ubicación GPS como válida.
        /// Viene de AppConfig: DRIVER_STALE_LOCATION_MINUTES
        /// </summary>
        public int StaleLocationMinutes { get; set; }

        /// <summary>
        /// Máximo de drivers a retornar en esta ronda.
        /// Viene de DispatchConfig: MaxDriversToNotifyPerRound
        /// </summary>
        public int MaxDrivers { get; set; }
    }
}