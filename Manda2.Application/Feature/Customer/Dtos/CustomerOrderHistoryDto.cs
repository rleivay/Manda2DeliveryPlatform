// PROPÓSITO: Historial de OrderGroups del cliente para su app.
//            Vista resumida: estado, monto, comercios involucrados, fecha.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Customer.Dtos
{/// <summary>
 /// Resumen de un OrderGroup en el historial del cliente.
 /// </summary>
    public class CustomerOrderHistoryDto
    {
        public int OrderGroupId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal ServiceFee { get; set; }
        public string DeliveryAddressText { get; set; } = string.Empty;
        public string? PaymentMethodName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Nombres de los comercios involucrados en el pedido.
        /// Ej: ["McDonald's", "Farmacia Cruz Verde"]
        /// </summary>
        public List<string> MerchantNames { get; set; } = new();
    }
}
