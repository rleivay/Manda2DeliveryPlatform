using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Dtos
{
    /// <summary>
    /// Resumen de la SubOrder para la vista del comercio.
    /// </summary>
    public class MerchantSubOrderSummaryDto
    {
        public int SubOrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal NetPayable { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
