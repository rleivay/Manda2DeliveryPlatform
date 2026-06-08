using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Commands
{
    public class AcceptSubOrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;

        /// <summary>
        /// Indica si TODAS las SubOrders del grupo fueron aceptadas.
        /// Cuando es true, el OrderGroup puede avanzar a AwaitingDriverAssignment.
        /// </summary>
        public bool AllSubOrdersAccepted { get; set; }

        /// <summary>Nuevo estado del OrderGroup si avanzó.</summary>
        public string? NewOrderGroupStatus { get; set; }
    }
}
