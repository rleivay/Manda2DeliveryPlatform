using Manda2.Application.Common;
using Manda2.Application.Feature.Customer.Dtos;
using Manda2.Application.Feature.OrderGroups.Queries;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Customer.Queries
{
    /// <summary>
    /// Retorna el historial paginado de pedidos de un cliente.
    /// </summary>
    public class GetCustomerOrderHistoryQuery : IQuery<PagedResult<CustomerOrderHistoryDto>>
    {
        public int CustomerId { get; }
        public int Page { get; }
        public int PageSize { get; }

        public GetCustomerOrderHistoryQuery(int customerId, int page = 1, int pageSize = 20)
        {
            CustomerId = customerId;
            Page = page < 1 ? 1 : page;
            PageSize = pageSize > 50 ? 50 : pageSize < 1 ? 10 : pageSize;
        }
    }
}
