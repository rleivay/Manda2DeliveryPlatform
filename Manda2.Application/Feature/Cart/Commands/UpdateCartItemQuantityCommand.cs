using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Cart.Commands
{
    /// <summary>
    /// Actualiza la cantidad de un SubOrderDetail y recalcula totales en cascada.
    /// </summary>
    public record UpdateCartItemQuantityCommand(
        int SubOrderDetailId,
        int NewQuantity,
        int CustomerId
    ) : ICommand<UpdateCartItemQuantityResult>;
}
