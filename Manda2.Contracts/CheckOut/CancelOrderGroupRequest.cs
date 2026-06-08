using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    /// <summary>DTO de entrada para POST /api/checkout/cancel</summary>
    public class CancelOrderGroupRequest
    {
        /// <summary>ID del OrderGroup a cancelar.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>Motivo de cancelación (opcional para Customer).</summary>
        public string? CancellationReason { get; set; }
    }
}
