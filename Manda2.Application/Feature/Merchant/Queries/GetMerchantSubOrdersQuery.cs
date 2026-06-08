// PROPÓSITO: El comercio consulta sus SubOrders activas (cola de trabajo).
//            Muestra los pedidos pendientes de aceptar, en preparación
//            y listos para pickup.
//
// CONSUMIDOR: MerchantsController → GET api/merchants/{merchantId}/suborders
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.Merchant.Dtos;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Queries
{
    /// <summary>
    /// Consulta para obtener la cola de trabajo activa del comercio.
    /// </summary>
    public class GetMerchantSubOrdersQuery : IQuery<List<MerchantSubOrderDto>>
    {
        /// <summary>ID del comercio que consulta su cola.</summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// Filtrar por estado específico (opcional).
        /// Si es null, retorna todos los estados activos.
        /// </summary>
        public string? StatusFilter { get; set; }
    }
}
