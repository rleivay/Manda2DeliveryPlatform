// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Queries/GetAvailableOrders/
//          GetAvailableOrdersQuery.cs
//
// PROPÓSITO: Define el contrato de entrada para la consulta de pedidos
//            disponibles cercanos al driver.
//
// PATRÓN: IQuery<T> del mediador propio Manda2.Application.Mediator
//         (NO MediatR). Consistente con GetMerchantProductsQuery.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.DTOs;
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
    /// Query para obtener los grupos de órdenes disponibles
    /// cercanos a la posición GPS actual del driver.
    ///
    /// CUÁNDO SE USA:
    /// - La app del Driver hace polling mientras está en estado "Available"
    /// - Al recibir una notificación push de nuevo pedido disponible
    /// </summary>
    public record GetAvailableOrdersQuery(
        int DriverId,
        decimal CurrentLatitude,
        decimal CurrentLongitude
    ) : IQuery<List<OrderGroupSummaryDto>>;
}
