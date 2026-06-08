using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = null!;

        public int CustomerId { get; set; }

        
        // FINANCIERO CONSOLIDADO
        public decimal ServiceFee { get; set; }
        public decimal DeliveryFee { get; set; }

        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; } = null!; // Cash, Card, Transfer

        public string Status { get; set; } = null!;

        // SAP READY
        public int? SAP_DocEntry_ARInvoice { get; set; }

        // RELACIONES
        public ICollection<SubOrder> SubOrders { get; set; } = new List<SubOrder>();
    }
}
