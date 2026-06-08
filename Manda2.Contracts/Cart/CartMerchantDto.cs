using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Cart
{
    public class CartMerchantDto
    {
        public int MerchantId { get; set; }
        public string MerchantName { get; set; } = string.Empty;

        /// <summary>SubOrder.SubTotal — suma de LineTotal de los items de este comercio.</summary>
        public decimal Subtotal { get; set; }

        /// <summary>Items del comercio.</summary>
        public List<CartItemDto> Items { get; set; } = new();
    }
}
