// PROPÓSITO: Consulta paginada de OrderGroups filtrada por Status.
//            Consumidor principal: BackOffice (panel de monitoreo de pedidos).
//
// PARÁMETROS:
//   Status     → Filtro obligatorio (ej: "AwaitingDriverAssignment")
//   Page       → Página actual (base 1)
//   PageSize   → Registros por página (máx 50)

using Manda2.Application.Common;
using Manda2.Application.Feature.OrderGroups.Dtos;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Queries
{
    /// <summary>
    /// Retorna una lista paginada de OrderGroups filtrada por estado.
    /// </summary>
    public class GetOrderGroupsByStatusQuery : IQuery<PagedResult<OrderGroupSummaryDto>>
    {
        /// <summary>Estado a filtrar. Debe coincidir con OrderGroupStatus enum.</summary>
        public string Status { get; }

        /// <summary>Página actual (base 1).</summary>
        public int Page { get; }

        /// <summary>Registros por página. Máximo 50.</summary>
        public int PageSize { get; }

        public GetOrderGroupsByStatusQuery(string status, int page = 1, int pageSize = 20)
        {
            Status = status;
            Page = page < 1 ? 1 : page;
            PageSize = pageSize > 50 ? 50 : pageSize < 1 ? 10 : pageSize;
        }
    }
}
