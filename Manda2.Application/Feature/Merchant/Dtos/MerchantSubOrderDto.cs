using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Dtos
{
    /// <summary>
    /// DTO de SubOrder para la vista del comercio.
    /// Contiene solo la información relevante para operar.
    /// </summary>
    public class MerchantSubOrderDto
    {
        public int SubOrderId { get; set; }
        public int OrderGroupId { get; set; }

        public string Status { get; set; } = null!;

        /// <summary>Subtotal de esta suborden (lo que el comercio prepara).</summary>
        public decimal SubTotal { get; set; }

        /// <summary>Monto neto que recibirá el comercio después de comisión.</summary>
        public decimal NetPayable { get; set; }

        /// <summary>Fecha y hora en que se creó la suborden.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Fecha de aceptación (si ya fue aceptada).</summary>
        public DateTime? AcceptedAt { get; set; }

        /// <summary>Fecha en que se marcó lista para pickup.</summary>
        public DateTime? ReadyAt { get; set; }

        /// <summary>Líneas de detalle del pedido (productos).</summary>
        public List<MerchantSubOrderDetailDto> Details { get; set; } = new();
    }
}
