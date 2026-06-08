using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Cart.Commands
{
    public record RemoveCartItemResult(
        bool Success,
        string Message,
        bool SubOrderRemoved,
        bool OrderGroupRemoved
    );
}
