using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    /// <summary>
    /// Proyección de PaymentMethod para consumo desde la app móvil.
    /// </summary>
    public class PaymentMethodDto
    {
        /// <summary>PK del método de pago.</summary>
        public int Id { get; set; }

        /// <summary>Código interno. Ej: "CASH", "CARD", "TRANSFER".</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Nombre visible. Ej: "Efectivo", "Tarjeta", "Transferencia".</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el método requiere número de referencia.
        /// El Mobile debe mostrar campo de referencia si es true.
        /// </summary>
        public bool RequiresReference { get; set; }

        /// <summary>
        /// Indica si el método requiere comprobante (voucher/foto).
        /// El Mobile debe habilitar upload de imagen si es true.
        /// </summary>
        public bool RequiresVoucher { get; set; }
    }
}
