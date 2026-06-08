// PROPÓSITO: El comercio consulta sus OrderGroups con su SubOrder asociada.
//            Filtro opcional por estado de SubOrder.
//            Paginado para BackOffice y app del comercio.

using Manda2.Application.Common;
using Manda2.Application.Feature.Merchant.Dtos;
using Manda2.Application.Feature.OrderGroups.Queries;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Queries
{
    /// <summary>
    /// Consulta paginada de OrderGroups que contienen una SubOrder del comercio.
    /// </summary>
    public class GetMerchantOrdersQuery : IQuery<PagedResult<MerchantOrderGroupDto>>
    {
        /// <summary>ID del comercio que consulta sus pedidos.</summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// Filtro opcional por estado de SubOrder.
        /// Si es null, retorna todos los estados.
        /// </summary>
        public string? SubOrderStatusFilter { get; set; }

        /// <summary>Página actual (base 1).</summary>
        public int Page { get; set; } = 1;

        /// <summary>Registros por página. Máximo 50.</summary>
        public int PageSize { get; set; } = 20;
    }
}
