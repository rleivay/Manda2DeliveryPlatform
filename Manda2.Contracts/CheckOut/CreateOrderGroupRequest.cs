using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    /// <summary>DTO de entrada para POST /api/checkout/create-order-group</summary>
    public class CreateOrderGroupRequest
    {
        /// <summary>Dirección de entrega en texto libre.</summary>
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>Latitud del punto de entrega.</summary>
        public decimal Lat { get; set; }

        /// <summary>Longitud del punto de entrega.</summary>
        public decimal Lon { get; set; }

        /// <summary>Notas generales del pedido (opcional).</summary>
        public string? Notes { get; set; }

        /// <summary>Lista de comercios con sus productos.</summary>
        public List<MerchantCartRequest> Merchants { get; set; } = new();
    }
}
