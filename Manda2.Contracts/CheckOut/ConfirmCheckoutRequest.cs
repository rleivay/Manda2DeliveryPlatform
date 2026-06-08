using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    /// <summary>DTO de entrada para POST /api/checkout</summary>
    public class ConfirmCheckoutRequest
    {
        /// <summary>ID del OrderGroup en Draft a confirmar.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>ID del método de pago (FK a cfg.PaymentMethods).</summary>
        public int PaymentMethodId { get; set; }

        /// <summary>Monto declarado. Debe cubrir OrderGroup.TotalAmount.</summary>
        public decimal Amount { get; set; }

        /// <summary>URL del comprobante. Requerido solo para Transferencia.</summary>
        public string? ProofUrl { get; set; }

        /// <summary>Referencia bancaria. Requerido para Transferencia.</summary>
        public string? Reference { get; set; }
    }
}
