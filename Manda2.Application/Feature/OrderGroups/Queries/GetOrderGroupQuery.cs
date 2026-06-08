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
    /// Consulta el estado detallado de un OrderGroup.
    /// Consumidores: Cliente (tracking), Driver (ruta activa), BackOffice.
    /// </summary>
    public class GetOrderGroupQuery : IQuery<OrderGroupDetailDto>
    {
        public int OrderGroupId { get; }

        public GetOrderGroupQuery(int orderGroupId)
        {
            OrderGroupId = orderGroupId;
        }
    }
}
