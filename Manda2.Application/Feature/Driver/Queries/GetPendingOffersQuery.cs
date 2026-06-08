// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Queries/GetPendingOffers/
//          GetPendingOffersQuery.cs
//
// PROPÓSITO: Define los parámetros para consultar ofertas disponibles.
//            Requiere la posición GPS del driver para calcular distancias
//            en tiempo real y mostrar las ofertas más cercanas primero.
//
// PATRÓN: IQuery<TResult> — Operación de solo lectura.
// CONSUMIDOR: DriverController → GET /api/driver/offers?lat=...&lon=...
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Queries
{
    /// <summary>
    /// Consulta para obtener las ofertas de grupos de pedidos que han sido
    /// notificadas a un repartidor específico y siguen pendientes.
    /// </summary>
    public class GetPendingOffersQuery : IQuery<List<PendingOfferDto>>
    {
        /// <summary>
        /// ID del repartidor que consulta sus ofertas.
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// Posición actual del repartidor para cálculo de distancias.
        /// </summary>
        public decimal CurrentLatitude { get; set; }
        public decimal CurrentLongitude { get; set; }
    }
}
