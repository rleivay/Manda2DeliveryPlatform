using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Dtos
{
    /// <summary>
    /// Línea de producto dentro de la SubOrder (vista del comercio).
    /// </summary>
    public class MerchantSubOrderDetailDto
    {
        public int DetailId { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal LineTotal { get; set; }

        /// <summary>Notas especiales del cliente para este ítem.</summary>
        public string? Notes { get; set; }
    }
}
