// PROPÓSITO: Vista del OrderGroup completo desde la perspectiva del comercio.
//            Muestra el grupo padre + la SubOrder que le pertenece.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Dtos
{
    /// <summary>
    /// Vista del OrderGroup desde el comercio: incluye datos del grupo
    /// y la SubOrder específica de ese comercio.
    /// </summary>
    public class MerchantOrderGroupDto
    {
        public int OrderGroupId { get; set; }
        public string OrderGroupStatus { get; set; } = string.Empty;
        public string DeliveryAddressText { get; set; } = string.Empty;
        public string? PaymentMethodName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        /// <summary>SubOrder específica de este comercio dentro del grupo.</summary>
        public MerchantSubOrderSummaryDto SubOrder { get; set; } = null!;
    }
}
