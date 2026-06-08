using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    /// <summary>
    /// Resultado del comando CancelOrderGroupCommand.
    /// </summary>
    public class CancelOrderGroupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>True si el driver fue liberado como parte de la cancelación.</summary>
        public bool DriverReleased { get; set; }

        /// <summary>Número de SubOrders canceladas.</summary>
        public int SubOrdersCancelled { get; set; }
    }
}
