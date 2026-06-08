using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Cart.Commands
{
    public record RemoveCartItemCommand(
         int SubOrderDetailId,
         int CustomerId
     ) : ICommand<RemoveCartItemResult>;
}
