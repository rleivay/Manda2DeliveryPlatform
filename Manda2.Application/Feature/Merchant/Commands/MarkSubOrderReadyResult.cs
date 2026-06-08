using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Commands
{
    public class MarkSubOrderReadyResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;

        /// <summary>
        /// True si TODAS las SubOrders del grupo están ReadyForPickup.
        /// Útil para que el driver sepa que puede iniciar la ruta.
        /// </summary>
        public bool AllSubOrdersReady { get; set; }
    }
}
