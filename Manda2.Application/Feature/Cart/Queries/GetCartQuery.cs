using Manda2.Application.Mediator;
using Manda2.Contracts.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Cart.Queries
{
    /// <summary>
    /// Query para obtener el carrito de un OrderGroup específico.
    /// </summary>
    public record GetCartQuery(int OrderGroupId, int CustomerId) : IQuery<CartDto>;
}
