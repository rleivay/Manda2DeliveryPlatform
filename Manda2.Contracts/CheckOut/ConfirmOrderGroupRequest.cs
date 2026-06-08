using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PROPÓSITO: DTO de entrada para POST /api/checkout/confirm-order-group.
//            Recibe la selección de dirección del cliente.
// ════════════════════════════════════════════════════════════════════════════

namespace Manda2.Contracts.CheckOut
{
    /// <summary>
    /// DTO de entrada para la consolidación logística del checkout.
    /// </summary>
    public class ConfirmOrderGroupRequest
    {
        /// <summary>ID del OrderGroup en estado Draft a consolidar.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>
        /// ID de la ShippingAddress seleccionada por el cliente.
        /// Debe pertenecer al cliente autenticado y estar activa.
        /// </summary>
        public int ShippingAddressId { get; set; }
    }
}
