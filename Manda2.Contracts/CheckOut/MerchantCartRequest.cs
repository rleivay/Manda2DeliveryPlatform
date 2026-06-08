using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    public class MerchantCartRequest
    {
        public int MerchantId { get; set; }
        public List<CartItemRequest> Items { get; set; } = new();
    }
}
