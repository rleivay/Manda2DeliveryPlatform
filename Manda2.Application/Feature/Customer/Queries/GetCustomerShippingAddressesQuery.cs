using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Contracts.Customer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Customer.Queries
{
    /// <summary>
    /// Retorna las direcciones de envío activas del cliente autenticado.
    /// </summary>
    public record GetCustomerShippingAddressesQuery(int CustomerId)
        : IQuery<List<ShippingAddressDto>>;

}
